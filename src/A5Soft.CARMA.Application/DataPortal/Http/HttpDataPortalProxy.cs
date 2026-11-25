using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.DataPortal.Client;
using A5Soft.CARMA.Application.DataPortal.EndPoints;
using A5Soft.CARMA.Application.DataPortal.Internal;
using A5Soft.CARMA.Application.DataPortal.Protocol;
using A5Soft.CARMA.Domain.Files;
using Newtonsoft.Json;

namespace A5Soft.CARMA.Application.DataPortal.Http
{
    /// <summary>
    /// The default transport of the data portal, over HTTP: every call is a POST to a single endpoint,
    /// with the call described in headers, the payload and the files as a multipart body, and the
    /// result (after an optional identity block) as the response body. Bodies are streamed both ways.
    /// A GET to the same endpoint gets the <see cref="ServerSettings"/>.
    /// </summary>
    /// <remarks>
    /// <para>Created by <see cref="HttpDataPortalTransport"/>, one per endpoint, and shared by every user and
    /// scope. Every proxy sends through one static <see cref="HttpClient"/>, an implementation detail: nothing
    /// about a call depends on the client (the server is the request URL, authentication is in the call), and
    /// one client keeps one connection pool per host for the life of the app. Its message handler is the
    /// platform's, unless the app supplies one (<see cref="IoC.IoCManagerBase.CreateHttpMessageHandler"/>).</para>
    /// <para>A call with files asks <c>Expect: 100-continue</c>, so that the server can refuse it before
    /// the files are sent. The server's refusal comes as 417 Expectation Failed. A 417 without the
    /// portal's result header comes from an intermediary that does not support the expectation: the
    /// call is sent again without it, which is safe because no file has been read yet.</para>
    /// <para>On .NET, the body is sent anyway if the server has not answered within the handler's
    /// <c>Expect100ContinueTimeout</c> (1 second by default; an app can supply a handler with a longer one): if
    /// verifying a token takes longer, a refused call has transferred part of its files first; the result is the
    /// same. <see cref="IHttpDataPortalClientOptions.CallTimeout"/> limits each call until the response headers
    /// arrive, the upload included.</para>
    /// <para>In a browser (Blazor WebAssembly), <c>Expect</c> is a forbidden header and is never sent:
    /// a refused call has always transferred its files first. Response streaming is enabled on every
    /// request; request streaming is an option (<see cref="IHttpDataPortalClientOptions.BrowserRequestStreaming"/>).
    /// The settings GET carries no custom header, so it needs no CORS preflight.</para>
    /// </remarks>
    public sealed class HttpDataPortalProxy : IDataPortalProxy
    {
        private const string BrowserResponseStreamingOption = "WebAssemblyEnableStreamingResponse";
        private const string BrowserRequestStreamingOption = "WebAssemblyEnableStreamingRequest";
        private const string PayloadContentType = "application/json";
        private const int MaxServerSettingsLength = 64 * 1024;

        private static readonly bool IsBrowser = RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"));
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        private static readonly object ClientLock = new object();
        private static HttpMessageHandler _handler;
        private static HttpClient _client;

        private readonly Uri _endpoint;
        private readonly IDataPortalClientOptions _options;
        private readonly IHttpDataPortalClientOptions _httpOptions;


        /// <summary>
        /// Creates a new HTTP proxy.
        /// </summary>
        /// <param name="endpoint">the absolute URL of the server data portal endpoint</param>
        /// <param name="options">the client data portal options</param>
        /// <param name="httpOptions">the client HTTP transport options; null for the defaults</param>
        public HttpDataPortalProxy(Uri endpoint, IDataPortalClientOptions options,
            IHttpDataPortalClientOptions httpOptions = null)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            if (!endpoint.IsAbsoluteUri) throw new ArgumentException(
                "The data portal endpoint must be an absolute URL.", nameof(endpoint));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpOptions = httpOptions ?? new HttpDataPortalClientOptions();
        }


        /// <inheritdoc />
        public bool IsRemote => true;


        /// <summary>
        /// Sets the message handler every HTTP data portal call is sent through. Called only by
        /// <see cref="IoC.IoCManagerBase"/>, from <see cref="IoC.IoCManagerBase.CreateHttpMessageHandler"/>: the one
        /// place the app configures the connection (e.g. a <c>SocketsHttpHandler</c> with a connection lifetime, an
        /// <c>Expect100ContinueTimeout</c> or a proxy server). Without it, the platform's handler is used.
        /// </summary>
        /// <param name="handler">the handler; the proxy owns it from now on and never disposes it</param>
        /// <remarks>Only the handler can be set: headers, authorization and timeouts belong to each request and are
        /// set by the proxy.</remarks>
        /// <exception cref="InvalidOperationException">already configured, or a call has already been sent</exception>
        /// <exception cref="NotSupportedException">in a browser, whose own handler must stay</exception>
        internal static void ConfigureHandler(HttpMessageHandler handler)
        {
            if (null == handler) throw new ArgumentNullException(nameof(handler));
            if (IsBrowser) throw new NotSupportedException(
                "In a browser the HTTP data portal must send through the browser's own handler.");

            lock (ClientLock)
            {
                if (null != _client) throw new InvalidOperationException(
                    "The HTTP data portal client is already in use: its handler can only be set at startup, " +
                    "before the first call.");
                if (null != _handler) throw new InvalidOperationException(
                    "The HTTP data portal handler is already set; it can be set only once per process.");

                _handler = handler;
            }
        }

        /// <summary>
        /// Gets the client, created on first use with the configured handler, or the platform's default. Never
        /// configured through default headers or properties: every request carries its own, so nothing one call sets
        /// can leak into another; no timeout of its own, as <see cref="IHttpDataPortalClientOptions.CallTimeout"/>
        /// is applied per call.
        /// </summary>
        private static HttpClient Client
        {
            get
            {
                var client = Volatile.Read(ref _client);
                if (null != client) return client;

                lock (ClientLock)
                {
                    if (null == _client)
                    {
                        var created = null == _handler ? new HttpClient() : new HttpClient(_handler, false);
                        created.Timeout = Timeout.InfiniteTimeSpan;
                        Volatile.Write(ref _client, created);
                    }

                    return _client;
                }
            }
        }


        /// <inheritdoc />
        public async Task<DataPortalResponse> SendAsync(DataPortalRequest request,
            CancellationToken cancellationToken = default)
        {
            if (null == request) throw new ArgumentNullException(nameof(request));

            var headers = CreateHeaders(request.Call);
            var expectContinue = request.Files.Count > 0 && !IsBrowser;

            Exchange exchange;
            try
            {
                exchange = await SendOnceAsync(request, headers, expectContinue, cancellationToken)
                    .ConfigureAwait(false);

                if ((int)exchange.Response.StatusCode == 417 && !IsPortalResponse(exchange.Response)
                    && expectContinue && !request.FilesRead)
                {
                    // an intermediary refused the expectation itself; the body was never sent
                    exchange.Dispose();
                    exchange = await SendOnceAsync(request, headers, false, cancellationToken)
                        .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is IOException || ex is OperationCanceledException)
            {
                // no response at all: the server is down or unreachable, or the client's timeout has passed
                throw new DataPortalTransportException(RemoteErrorKind.ServiceUnavailable,
                    "The data portal server could not be reached.", ex);
            }

            try
            {
                return await ReadResponseAsync(exchange, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch
            {
                exchange.Dispose();
                throw;
            }
        }


        /// <inheritdoc />
        public async Task<ServerSettings> GetServerSettingsAsync(CancellationToken cancellationToken = default)
        {
            Exchange exchange;
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, _endpoint);
                try
                {
                    httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(PayloadContentType));

                    var response = await SendWithTimeoutAsync(httpRequest, cancellationToken)
                        .ConfigureAwait(false);

                    exchange = new Exchange(httpRequest, response);
                }
                catch
                {
                    httpRequest.Dispose();
                    throw;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is IOException || ex is OperationCanceledException)
            {
                throw new DataPortalTransportException(RemoteErrorKind.ServiceUnavailable,
                    "The data portal server could not be reached.", ex);
            }

            using (exchange)
            {
                var response = exchange.Response;
                var status = (int)response.StatusCode;
                var protocol = GetHeader(response, DataPortalHeaders.Protocol)?.Trim();

                if (status != 200 || null == protocol) throw new DataPortalTransportException(
                    status == 408 || status == 502 || status == 503 || status == 504
                        ? RemoteErrorKind.ServiceUnavailable
                        : RemoteErrorKind.ServerError,
                    $"The server answered the settings request with HTTP status {status} " +
                    (null == protocol ? "without a data portal protocol version." : "."));

                if (protocol != DataPortalHeaders.CurrentProtocolVersion) throw new DataPortalTransportException(
                    RemoteErrorKind.VersionMismatch,
                    $"The server speaks data portal protocol {protocol}, the client {DataPortalHeaders.CurrentProtocolVersion}.");

                try
                {
                    var body = null == response.Content
                        ? Stream.Null
                        : await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                    using (var json = await body.ReadToEndAsync(MaxServerSettingsLength, cancellationToken)
                        .ConfigureAwait(false)
                        ?? throw Malformed($"settings longer than {MaxServerSettingsLength} bytes"))
                    using (var textReader = new StreamReader(json, Utf8))
                    using (var jsonReader = new JsonTextReader(textReader) { MaxDepth = DataPortalSerialization.PortalMessageMaxDepth })
                    {
                        return DataPortalSerialization.CreateSerializer(DataPortalSerialization.PortalMessageMaxDepth)
                            .Deserialize<ServerSettings>(jsonReader) ?? throw Malformed("empty settings");
                    }
                }
                catch (JsonException ex)
                {
                    throw Malformed("settings that are not valid", ex);
                }
                catch (IOException ex)
                {
                    throw new DataPortalTransportException(RemoteErrorKind.ServiceUnavailable,
                        "The connection to the data portal server was lost.", ex);
                }
            }
        }


        #region Request

        /// <summary>
        /// Creates the headers of a call: the protocol, the use case, its parameter types and generic arguments, the
        /// cultures, the plugin fingerprint, the file count and, for a call that is not anonymous, the ticket.
        /// </summary>
        /// <param name="call">the call header</param>
        /// <exception cref="InvalidOperationException">the ticket cannot travel in an HTTP header</exception>
        private Dictionary<string, string> CreateHeaders(DataPortalCall call)
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [DataPortalHeaders.Protocol] = DataPortalHeaders.CurrentProtocolVersion,
                [DataPortalHeaders.UseCase] = DataPortalHeaderValues.EncodeText(call.UseCase),
                [DataPortalHeaders.ParameterTypes] = DataPortalHeaderValues.EncodeList(call.ParameterTypes),
                [DataPortalHeaders.Culture] = DataPortalHeaderValues.EncodeText(call.Culture ?? string.Empty),
                [DataPortalHeaders.UICulture] = DataPortalHeaderValues.EncodeText(call.UICulture ?? string.Empty),
                [DataPortalHeaders.FileCount] = DataPortalHeaderValues.EncodeNumber(call.FileCount)
            };

            if (call.GenericArguments.Count > 0)
                headers[DataPortalHeaders.GenericArguments] = DataPortalHeaderValues.EncodeList(call.GenericArguments);

            if (null != call.PluginFingerprint)
                headers[DataPortalHeaders.Plugins] = DataPortalHeaderValues.EncodeText(call.PluginFingerprint);

            if (null != call.Ticket)
            {
                if (call.Ticket.Length < 1 || call.Ticket.Any(c => c < 0x21 || c > 0x7E)) throw new InvalidOperationException(
                    "The token must consist of printable ASCII characters without spaces to travel in an HTTP header; encode its fields.");

                headers["Authorization"] = DataPortalHeaders.AuthorizationScheme + " " + call.Ticket;
            }

            // a proxy would refuse oversized headers with a bare 400 or 431
            var total = 0;
            foreach (var header in headers)
            {
                if (header.Value.Length > _httpOptions.MaxRequestHeaderLength) throw new InvalidOperationException(
                    $"Header {header.Key} is {header.Value.Length} characters long, more than the " +
                    $"{_httpOptions.MaxRequestHeaderLength} allowed.");

                total += header.Key.Length + header.Value.Length + 4;
            }

            if (total > _httpOptions.MaxRequestHeadersLength) throw new InvalidOperationException(
                $"The request headers are {total} characters long, more than the " +
                $"{_httpOptions.MaxRequestHeadersLength} allowed.");

            return headers;
        }

        /// <summary>
        /// Sends a request through the shared client until the response headers arrive, within
        /// <see cref="IHttpDataPortalClientOptions.CallTimeout"/>; the body of a response is read afterwards.
        /// </summary>
        /// <exception cref="DataPortalTransportException">the timeout has passed</exception>
        private async Task<HttpResponseMessage> SendWithTimeoutAsync(HttpRequestMessage httpRequest,
            CancellationToken cancellationToken)
        {
            var timeout = _httpOptions.CallTimeout;
            if (timeout == Timeout.InfiniteTimeSpan) return await Client.SendAsync(httpRequest,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

            using (var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                timeoutSource.CancelAfter(timeout);
                try
                {
                    return await Client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead,
                        timeoutSource.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    // the caller's cancellation still comes out as OperationCanceledException
                    throw new DataPortalTransportException(RemoteErrorKind.ServiceUnavailable,
                        $"The data portal server did not answer within {timeout}.", ex);
                }
            }
        }

        private async Task<Exchange> SendOnceAsync(DataPortalRequest request,
            Dictionary<string, string> headers, bool expectContinue, CancellationToken cancellationToken)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _endpoint) { Content = CreateContent(request) };
            try
            {
                foreach (var header in headers)
                {
                    if (!httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value))
                        throw new InvalidOperationException($"Header {header.Key} cannot be sent.");
                }

                if (expectContinue) httpRequest.Headers.ExpectContinue = true;

                if (IsBrowser)
                {
#pragma warning disable CS0618 // Properties is Options on .NET 5+; Options is not in .NET Standard
                    httpRequest.Properties[BrowserResponseStreamingOption] = true;
                    if (_httpOptions.BrowserRequestStreaming) httpRequest.Properties[BrowserRequestStreamingOption] = true;
#pragma warning restore CS0618
                }

                var response = await SendWithTimeoutAsync(httpRequest, cancellationToken)
                    .ConfigureAwait(false);

                return new Exchange(httpRequest, response);
            }
            catch
            {
                httpRequest.Dispose();
                throw;
            }
        }

        /// <summary>
        /// The multipart body: the payload part first, then a part per file, in reference order.
        /// The files are streamed as they are sent; the length is set only when every part's is known,
        /// otherwise the body is sent chunked.
        /// </summary>
        private HttpContent CreateContent(DataPortalRequest request)
        {
            var content = new MultipartFormDataContent("carma-" + Guid.NewGuid().ToString("N"));

            // sends the memory as it is, no copy; its length is known, so the part gets a Content-Length
            var payload = new ReadOnlyMemoryContent(request.Payload);
            payload.Headers.ContentType = new MediaTypeHeaderValue(PayloadContentType) { CharSet = "utf-8" };
            content.Add(payload, DataPortalHeaders.PayloadPartName);

            for (var i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];

                // disposing the content disposes this stream, which never disposes the caller's file
                var part = new StreamContent(file.Content, _options.CopyBufferSize);
                part.Headers.ContentType = MediaTypeHeaderValue.TryParse(file.ContentType, out var type)
                    ? type
                    : new MediaTypeHeaderValue(FileContentType.Binary);
                if (file.Length.HasValue) part.Headers.ContentLength = file.Length;

                // sets both filename and the RFC 5987 filename* for non-ASCII names
                content.Add(part, DataPortalHeaders.FilePartName(i), file.FileName);
            }

            return content;
        }

        #endregion

        #region Response

        private static bool IsPortalResponse(HttpResponseMessage response)
            => response.Headers.Contains(DataPortalHeaders.Result);

        private static string GetHeader(HttpResponseMessage response, string name)
            => response.Headers.TryGetValues(name, out var values) ? string.Join(",", values) : null;

        private async Task<DataPortalResponse> ReadResponseAsync(Exchange exchange,
            CancellationToken cancellationToken)
        {
            var response = exchange.Response;
            var status = (int)response.StatusCode;
            var kind = GetHeader(response, DataPortalHeaders.Result)?.Trim();

            // every portal outcome is a 200 with the result header, or a 417 refusing the files;
            // anything else is the infrastructure
            if (null == kind || (status != 200 && status != 417))
                throw new DataPortalTransportException(
                    status == 408 || status == 502 || status == 503 || status == 504
                        ? RemoteErrorKind.ServiceUnavailable
                        : RemoteErrorKind.ServerError,
                    $"The server answered with HTTP status {status} without a data portal result.");

            var body = null == response.Content
                ? Stream.Null
                : await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            if (kind == DataPortalHeaders.ResultKinds.Error)
            {
                var error = await ReadErrorAsync(body, cancellationToken).ConfigureAwait(false);
                exchange.Dispose();
                return DataPortalResponse.ForError(error, status == 417);
            }

            if (status != 200) throw Malformed("a refusal that is not an error");

            var identity = await ReadIdentityAsync(response, body, cancellationToken)
                .ConfigureAwait(false);

            switch (kind)
            {
                case DataPortalHeaders.ResultKinds.None:
                    exchange.Dispose();
                    return DataPortalResponse.ForNone(identity);

                case DataPortalHeaders.ResultKinds.Value:
                    var resultType = DataPortalHeaderValues.DecodeText(GetHeader(response, DataPortalHeaders.ResultType))
                        ?? throw Malformed("a value without its type");
                    var value = await body.ReadToEndAsync(_options.MaxResponseLength, cancellationToken).ConfigureAwait(false)
                        ?? throw Malformed($"a value longer than {_options.MaxResponseLength} bytes");
                    exchange.Dispose();
                    // over the buffer the body was read into, no copy. Nothing to release: a MemoryStream
                    // holds managed memory only, which lives as long as the response refers to it
                    return DataPortalResponse.ForValue(resultType, value.WrittenBytes(), identity);

                case DataPortalHeaders.ResultKinds.File:
                    // the file reads the rest of the body as it arrives, and releases the response
                    // (and with it the connection) when it is disposed
                    var file = new FileContent(
                        new OwningReadStream(body,
                            DataPortalHeaderValues.DecodeNumber(GetHeader(response, DataPortalHeaders.FileLength)), exchange),
                            Sanitizer.SanitizeContentType(DataPortalHeaderValues.DecodeText(GetHeader(response, DataPortalHeaders.FileContentType))),
                            Sanitizer.SanitizeFileName(DataPortalHeaderValues.DecodeText(GetHeader(response, DataPortalHeaders.FileName)), "download"));
                    return DataPortalResponse.ForFile(file, identity);

                default:
                    throw Malformed($"an unknown result kind '{kind}'");
            }
        }

        /// <summary>
        /// Reads the identity update before the result: opaque bytes, which the client data portal reads.
        /// </summary>
        /// <param name="response">the response, whose header gives the length of the update</param>
        /// <param name="body">the response body, positioned at the update</param>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        /// <returns>the identity update; empty if the response has none</returns>
        private async Task<ReadOnlyMemory<byte>> ReadIdentityAsync(HttpResponseMessage response, Stream body,
            CancellationToken cancellationToken)
        {
            var header = GetHeader(response, DataPortalHeaders.IdentityLength);
            if (null == header) return default;

            var length = DataPortalHeaderValues.DecodeNumber(header);
            if (null == length || length > _options.MaxIdentityLength) throw Malformed("an identity block of invalid length");

            var buffer = new byte[length.Value];
            if (!await body.ReadExactlyAsync(buffer, buffer.Length, cancellationToken).ConfigureAwait(false))
                throw Malformed("a body shorter than its identity block");

            return buffer;
        }

        private async Task<RemoteError> ReadErrorAsync(Stream body, CancellationToken cancellationToken)
        {
            try
            {
                using (var json = await body.ReadToEndAsync(_options.MaxResponseLength, cancellationToken)
                    .ConfigureAwait(false))
                {
                    if (null == json) return new RemoteError { Kind = RemoteErrorKind.ServerError };

                    using (var textReader = new StreamReader(json, Utf8))
                    using (var jsonReader = new JsonTextReader(textReader) { MaxDepth = DataPortalSerialization.PortalMessageMaxDepth })
                    {
                        return DataPortalSerialization.CreateSerializer(DataPortalSerialization.PortalMessageMaxDepth)
                            .Deserialize<RemoteError>(jsonReader)
                            ?? new RemoteError { Kind = RemoteErrorKind.ServerError };
                    }
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                return new RemoteError { Kind = RemoteErrorKind.ServerError };
            }
        }

        private static DataPortalTransportException Malformed(string what, Exception inner = null)
            => new DataPortalTransportException(RemoteErrorKind.BadRequest,
                $"The server answered with {what}; the client and the server are probably of different versions.", inner);

        #endregion


        /// <summary>
        /// One sending of a call: keeps the request alive as long as the response (the server may answer
        /// before the request body has been sent), and releases both.
        /// </summary>
        private sealed class Exchange : IDisposable
        {
            private readonly HttpRequestMessage _request;
            private int _disposed;

            public Exchange(HttpRequestMessage request, HttpResponseMessage response)
            {
                _request = request;
                Response = response;
            }

            public HttpResponseMessage Response { get; }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

                Response.Dispose();
                _request.Dispose();
            }
        }
    }
}
