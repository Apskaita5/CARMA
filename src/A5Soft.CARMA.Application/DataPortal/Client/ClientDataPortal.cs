using A5Soft.CARMA.Application.Authorization.Authorizers;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.DataPortal.EndPoints;
using A5Soft.CARMA.Application.DataPortal.Internal;
using A5Soft.CARMA.Application.DataPortal.Protocol;
using A5Soft.CARMA.Application.IoC.Plugins;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Files;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal.Client
{
    /// <summary>
    /// The client side of the data portal. Turns a use case call into a canonical, transport
    /// independent request, hands it to the <see cref="IDataPortalProxy"/>, and turns the canonical
    /// response back into the result.
    /// </summary>
    /// <remarks>
    /// <para>Domain objects are sent as their DTOs and rebuilt from the DTOs received, with the
    /// client's own <see cref="IApplicationContext"/>.</para>
    /// <para>Uploads: any <see cref="FileContent"/> in the parameters (top-level, in a DTO, in a
    /// list) is detached and handed to the proxy as a <see cref="DataPortalFile"/>; the server
    /// reattaches it. The caller keeps owning the files; each stream is read once, as it is sent.</para>
    /// <para>Downloads: a <see cref="FileContent"/> result streams as it arrives, and the caller must
    /// dispose it to release the transport. A file nested in a result always arrives as null: files
    /// travel upstream only.</para>
    /// <para>Before the first call, the client fetches the <see cref="ServerSettings"/>
    /// (<see cref="IServerSettingsProvider"/>) and compares the server's application with its own
    /// (<see cref="IAppVersionProvider"/>), as <see cref="IDataPortalClientOptions.AppVersionMatch"/>
    /// directs: a mismatch fails every call with the app's business exception (IExceptionFactory.VersionMismatch), without
    /// sending it.</para>
    /// <para>The server's limits are enforced before sending and, for files of unknown length, while
    /// sending, whatever the transport.</para>
    /// <para>A failure reported by the server is re-created by its class: a <see cref="BusinessException"/> with
    /// the server's message, a <see cref="NotAuthenticatedException"/>, or a <see cref="RemoteCallException"/> with
    /// the correlation id for a fault; each marked as logged, as the server has logged it. A server that cannot be
    /// reached is a <see cref="DataPortalUnavailableException"/>.</para>
    /// </remarks>
    [DefaultServiceImplementation(typeof(IClientDataPortal))]
    public class ClientDataPortal : IClientDataPortal
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        private readonly RemoteUseCaseRegistry _registry;
        private readonly IDataPortalConnection _connection;
        // the domain context only: the application context holds the client data portal itself, so depending
        // on it would make a circular dependency
        private readonly Domain.IDomainContext _context;
        private readonly IExceptionFactory _exceptions;
        private readonly IDataPortalClientOptions _options;
        private readonly IServerSettingsProvider _settingsProvider;
        private readonly IAppVersionProvider _versionProvider;
        private readonly PluginCatalog _plugins;
        private readonly IAuthenticationStateProvider _authentication;


        /// <summary>
        /// Creates a new client data portal.
        /// </summary>
        /// <param name="registry">a registry of the remote use cases</param>
        /// <param name="context">the client application context, for the domain objects rebuilt
        /// from the DTOs received</param>
        /// <param name="exceptions">the app's factory of localized exceptions, for a version mismatch</param>
        /// <param name="options">the client data portal options</param>
        /// <param name="settingsProvider">a provider of the server's settings</param>
        /// <param name="versionProvider">the identity of the client application, compared with the
        /// server's</param>
        /// <param name="connection">the scope's data portal endpoint and its proxy</param>
        /// <param name="plugins">the plugins installed on the client, compared with the server's</param>
        /// <param name="authentication">the scope's identity: its ticket goes with every call, and updates are
        /// applied to it</param>
        public ClientDataPortal(IDataPortalConnection connection, RemoteUseCaseRegistry registry,
            Domain.IDomainContext context, IExceptionFactory exceptions, IDataPortalClientOptions options,
            IServerSettingsProvider settingsProvider, IAppVersionProvider versionProvider,
            PluginCatalog plugins, IAuthenticationStateProvider authentication)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _exceptions = exceptions ?? throw new ArgumentNullException(nameof(exceptions));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            _versionProvider = versionProvider ?? throw new ArgumentNullException(nameof(versionProvider));
            _plugins = plugins ?? throw new ArgumentNullException(nameof(plugins));
            _authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
        }


        /// <inheritdoc />
        public bool IsRemote => _connection.IsRemote;


        #region Fetch

        /// <inheritdoc />
        public Task<TResult> FetchAsync<TResult>(Type useCaseType, CancellationToken cancellationToken = default)
            => FetchCoreAsync<TResult>(useCaseType, null, cancellationToken);

        /// <inheritdoc />
        public Task<TResult> FetchAsync<TArg, TResult>(Type useCaseType, TArg arg,
            CancellationToken cancellationToken = default)
            => FetchCoreAsync<TResult>(useCaseType, null, cancellationToken, arg);

        /// <inheritdoc />
        public Task<TResult> FetchAsync<TArg, TResult>(Type useCaseType, TArg arg, Type[] genericArgs,
            CancellationToken cancellationToken = default)
            => FetchCoreAsync<TResult>(useCaseType, genericArgs, cancellationToken, arg);

        /// <inheritdoc />
        public Task<TResult> FetchAsync<TArg1, TArg2, TResult>(Type useCaseType, TArg1 arg1,
            TArg2 arg2, CancellationToken cancellationToken = default)
            => FetchCoreAsync<TResult>(useCaseType, null, cancellationToken, arg1, arg2);

        /// <inheritdoc />
        public Task<TResult> FetchAsync<TArg1, TArg2, TArg3, TResult>(Type useCaseType, TArg1 arg1,
            TArg2 arg2, TArg3 arg3, CancellationToken cancellationToken = default)
            => FetchCoreAsync<TResult>(useCaseType, null, cancellationToken, arg1, arg2, arg3);

        #endregion

        #region Invoke

        /// <inheritdoc />
        public Task InvokeAsync(Type useCaseType)
            => InvokeCoreAsync(useCaseType, CancellationToken.None);

        /// <inheritdoc />
        public Task InvokeAsync<TArg>(Type useCaseType, TArg arg)
            => InvokeCoreAsync(useCaseType, CancellationToken.None, arg);

        /// <inheritdoc />
        public Task InvokeAsync<TArg1, TArg2>(Type useCaseType, TArg1 arg1, TArg2 arg2)
            => InvokeCoreAsync(useCaseType, CancellationToken.None, arg1, arg2);

        /// <inheritdoc />
        public Task InvokeAsync<TArg1, TArg2, TArg3>(Type useCaseType, TArg1 arg1, TArg2 arg2, TArg3 arg3)
            => InvokeCoreAsync(useCaseType, CancellationToken.None, arg1, arg2, arg3);

        #endregion

        #region Core

        /// <summary>
        /// Invokes a remote method that returns a value. For a <see cref="FileContent"/> result,
        /// the caller must dispose it.
        /// </summary>
        /// <param name="useCaseType">a use case interface or implementation type</param>
        /// <param name="cancellationToken">a token to cancel the call</param>
        /// <param name="args">the arguments, in the order of the remote method signature</param>
        public Task<TResult> FetchCoreAsync<TResult>(Type useCaseType, CancellationToken cancellationToken, params object[] args)
            => FetchCoreAsync<TResult>(useCaseType, null, cancellationToken, args);

        /// <summary>
        /// Invokes a remote method that returns a value, closing a generic remote method over the generic
        /// arguments given. For a <see cref="FileContent"/> result, the caller must dispose it.
        /// </summary>
        /// <param name="useCaseType">a use case interface or implementation type</param>
        /// <param name="genericArgs">the generic arguments of a generic remote method, each of them a trusted
        /// wire type (<see cref="RemoteUseCaseRegistry.TrustedWireTypes"/>); null or empty for a method that is not generic</param>
        /// <param name="cancellationToken">a token to cancel the call</param>
        /// <param name="args">the arguments, in the order of the remote method signature</param>
        /// <exception cref="NotSupportedException">the generic arguments don't fit the method, or are not
        /// trusted wire types</exception>
        public async Task<TResult> FetchCoreAsync<TResult>(Type useCaseType, Type[] genericArgs,
            CancellationToken cancellationToken, params object[] args)
        {
            var descriptor = _registry.GetFor(useCaseType).Close(genericArgs);

            if (null == descriptor.Result || !typeof(TResult).IsAssignableFrom(descriptor.Result.DeclaredType))
                throw new InvalidOperationException(
                    $"Remote method {descriptor.Name}.{descriptor.Method.Name} does not return {typeof(TResult)}.");

            return (TResult)await CallAsync(descriptor, args, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Invokes a remote method. The result, if the method has one, is discarded
        /// (a downloaded file is released unread).
        /// </summary>
        /// <param name="useCaseType">a use case interface or implementation type</param>
        /// <param name="cancellationToken">a token to cancel the call</param>
        /// <param name="args">the arguments, in the order of the remote method signature</param>
        public async Task InvokeCoreAsync(Type useCaseType, CancellationToken cancellationToken, params object[] args)
        {
            var descriptor = _registry.GetFor(useCaseType).Close(Type.EmptyTypes);

            var result = await CallAsync(descriptor, args, cancellationToken)
                .ConfigureAwait(false);

            (result as IDisposable)?.Dispose();
        }

        /// <summary>
        /// Makes a call with the ticket of the scope's identity, and applies the identity update coming back before
        /// returning the result.
        /// </summary>
        /// <param name="descriptor">the remote method</param>
        /// <param name="args">the arguments, in the order of the remote method signature</param>
        /// <param name="cancellationToken">a token to cancel the call</param>
        /// <returns>the result; null for a method without one</returns>
        private async Task<object> CallAsync(RemoteUseCaseDescriptor descriptor, object[] args,
            CancellationToken cancellationToken)
        {
            // cancelling a call that changes state would leave the client not knowing whether it was made
            if (cancellationToken.CanBeCanceled && !descriptor.AcceptsCancellation) throw new ArgumentException(
                $"Remote method {descriptor.Name}.{descriptor.Method.Name} changes state, so it cannot be cancelled: " +
                $"only a method taking a {nameof(CancellationToken)} can.", nameof(cancellationToken));

            args = args ?? new object[] { null };

            if (args.Length != descriptor.Parameters.Count) throw new ArgumentException(
                $"Remote method {descriptor.Name}.{descriptor.Method.Name} takes " +
                $"{descriptor.Parameters.Count} parameter(s), {args.Length} given.", nameof(args));

            var settings = await GetServerSettingsAsync(cancellationToken)
                .ConfigureAwait(false);

            var identity = _authentication.Identity;
            var request = CreateRequest(descriptor, identity.Ticket, args, settings);

            DataPortalResponse response;
            try
            {
                response = await _connection.Proxy.SendAsync(request, cancellationToken).ConfigureAwait(false)
                    ?? throw new DataPortalTransportException(RemoteErrorKind.ServerError, "The proxy has returned no response.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception) when (null != request.LimitViolation)
            {
                // a file of unknown length turned out to be over the limits while it was sent
                throw LimitExceeded(request.LimitViolation);
            }
            catch (DataPortalTransportException ex)
            {
                if (ex.Kind == RemoteErrorKind.BadRequest || ex.Kind == RemoteErrorKind.VersionMismatch)
                    _settingsProvider.Invalidate(_connection.Endpoint);

                // the server speaks another version of the transport protocol
                if (ex.Kind == RemoteErrorKind.VersionMismatch) throw VersionMismatch(null, ex.Message, ex);

                throw ToException(ex);
            }

            if (null != request.LimitViolation)
            {
                // the server may have answered before the transport noticed that sending had failed
                response.Dispose();
                throw LimitExceeded(request.LimitViolation);
            }

            var (result, updatedIdentity) = ReadResponse(
                descriptor, response, settings, _authentication.Identity);

            // the identity first, so that whatever the caller does with the result sees it
            if (null != updatedIdentity)
                await _authentication.NotifyIdentityChangedAsync(updatedIdentity)
                    .ConfigureAwait(false);

            return result;
        }

        #endregion

        #region Server settings

        /// <summary>
        /// Gets the server's settings, and checks that the server runs the same application, in a
        /// version that matches.
        /// </summary>
        private async Task<ServerSettings> GetServerSettingsAsync(CancellationToken cancellationToken)
        {
            ServerSettings settings;
            try
            {
                settings = await _settingsProvider.GetAsync(_connection.Endpoint, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (DataPortalTransportException ex) when (ex.Kind == RemoteErrorKind.VersionMismatch)
            {
                // the server speaks another version of the transport protocol
                throw VersionMismatch(null, ex.Message, ex);
            }
            catch (DataPortalTransportException ex)
            {
                throw ToException(ex);
            }

            var reason = GetMismatch(settings);
            if (null != reason)
            {
                // the server may be updated again while the client stays as it is
                _settingsProvider.Invalidate(_connection.Endpoint);
                throw VersionMismatch(settings, reason);
            }

            return settings;
        }

        private string GetMismatch(ServerSettings settings)
        {
            var clientName = _versionProvider.AppName;
            var clientVersion = _versionProvider.AppVersion;

            if (string.IsNullOrWhiteSpace(clientName) || null == clientVersion) throw new InvalidOperationException(
                $"{nameof(IAppVersionProvider)} must give both the name and the version of the application.");

            if (!string.Equals(clientName, settings.AppName, StringComparison.Ordinal))
                return $"the server runs application '{settings.AppName}', the client is '{clientName}'";

            if (!IsMatch(clientVersion, settings.AppVersion, _options.AppVersionMatch))
                return $"the server runs version {settings.AppVersion}, the client is version {clientVersion} " +
                    $"({_options.AppVersionMatch} match required)";

            // GetMismatch, after the version check
            if (!string.Equals(_plugins.Fingerprint ?? string.Empty, settings.PluginFingerprint ?? string.Empty, StringComparison.Ordinal))
                return "the server and the client have different plugins installed";

            return null;
        }

        /// <summary>
        /// Compares two versions; a missing build or revision counts as 0, so 1.4 matches 1.4.0.0.
        /// </summary>
        internal static bool IsMatch(Version client, Version server, AppVersionMatch match)
        {
            switch (match)
            {
                case AppVersionMatch.None:
                    return true;

                case AppVersionMatch.MajorMinor:
                    return client.Major == server.Major && client.Minor == server.Minor;

                default:
                    return client.Major == server.Major && client.Minor == server.Minor
                        && Math.Max(client.Build, 0) == Math.Max(server.Build, 0)
                        && Math.Max(client.Revision, 0) == Math.Max(server.Revision, 0);
            }
        }

        private Exception VersionMismatch(ServerSettings settings, string reason, Exception innerException = null)
        {
            var data = new VersionMismatchData
            {
                ClientAppName = _versionProvider.AppName,
                ClientAppVersion = _versionProvider.AppVersion,
                ServerAppName = settings?.AppName,
                ServerAppVersion = settings?.AppVersion,
                AppVersionMatch = _options.AppVersionMatch,
                ClientPluginFingerprint = _plugins.Fingerprint,
                ServerPluginFingerprint = settings?.PluginFingerprint,
                Reason = reason
            };

            return _exceptions.VersionMismatch(data, innerException
                ?? new InvalidOperationException($"Version mismatch: {reason}."))
                ?? throw new InvalidOperationException(
                    $"{nameof(IExceptionFactory)}.{nameof(IExceptionFactory.VersionMismatch)} has returned no exception.");
        }

        #endregion

        #region Request

        /// <summary>
        /// Creates the request of a call: the call header with the ticket, the arguments as JSON and the files to upload.
        /// </summary>
        /// <param name="descriptor">the remote method</param>
        /// <param name="ticket">the caller's ticket; null for an anonymous call</param>
        /// <param name="args">the arguments, in the order of the remote method signature</param>
        /// <param name="settings">the server's settings</param>
        private DataPortalRequest CreateRequest(RemoteUseCaseDescriptor descriptor, string ticket,
            object[] args, ServerSettings settings)
        {
            // the parameters as JSON, with every file detached and replaced by a reference
            var files = new OutgoingFiles(_registry.TrustedWireTypes);
            var serializer = DataPortalSerialization.CreateSerializer(settings.MaxDepth);
            serializer.Context = new StreamingContext(StreamingContextStates.Remoting, files);

            var payload = new MemoryStream();
            using (var textWriter = new StreamWriter(payload, Utf8, 4096, true))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                jsonWriter.WriteStartArray();
                for (var i = 0; i < args.Length; i++) descriptor.Parameters[i].Write(jsonWriter, serializer, args[i]);
                jsonWriter.WriteEndArray();
            }

            CheckLimits(payload.Length, files.Files, settings);

            var call = new DataPortalCall(descriptor.Name, descriptor.ParameterTypeNames, ticket,
                CultureInfo.CurrentCulture.Name, CultureInfo.CurrentUICulture.Name, files.Files.Count,
                descriptor.GenericArgumentNames, _plugins.Fingerprint);

            // every file is counted as the proxy reads it, so a file of unknown length over a limit
            // is stopped before its excess is sent
            var tracker = new SendTracker(settings.MaxFileLength, settings.MaxUploadLength);
            var outgoing = files.Files
                .Select(f => new DataPortalFile(f.FileName, f.ContentType, tracker.Track(f.Content, f.FileName), KnownLength(f)))
                .ToList();

            // the payload is handed over as written, over the stream's own buffer: no copy (see PayloadBytes)
            return new DataPortalRequest(call, payload.WrittenBytes(), outgoing, tracker);
        }

        /// <summary>
        /// Checks the limits that can be checked before sending, so that a call the server would
        /// refuse is never transferred. A file of unknown length is checked as it is sent.
        /// </summary>
        private void CheckLimits(long payloadLength, IReadOnlyList<FileContent> files, ServerSettings limits)
        {
            string violation = null;

            if (payloadLength > limits.MaxPayloadLength)
                violation = $"the parameters take {payloadLength} bytes, more than the {limits.MaxPayloadLength} allowed";
            else if (files.Count > limits.MaxFileCount)
                violation = $"{files.Count} files are uploaded, more than the {limits.MaxFileCount} allowed";
            else
            {
                long total = 0;
                foreach (var file in files)
                {
                    var length = KnownLength(file);
                    if (length > limits.MaxFileLength)
                    {
                        violation = $"file {file.FileName} is {length} bytes long, more than the {limits.MaxFileLength} allowed";
                        break;
                    }

                    total += length ?? 0;
                }

                if (null == violation && total > limits.MaxUploadLength)
                    violation = $"the files are {total} bytes long, more than the {limits.MaxUploadLength} allowed";
            }

            if (null != violation) throw LimitExceeded(violation);
        }

        private static long? KnownLength(FileContent file)
            => file.Content.CanSeek ? file.Content.Length - file.Content.Position : (long?)null;

        #endregion

        #region Response

        /// <summary>
        /// Reads a response: throws its error, or gets its result and the caller's updated identity.
        /// </summary>
        /// <param name="descriptor">the remote method</param>
        /// <param name="response">the response</param>
        /// <param name="settings">the server's settings</param>
        /// <param name="identity">the identity the call was made with, which an update applies to</param>
        /// <returns>the result (null for none) and the updated identity (null if nothing has changed)</returns>
        private (object Result, CarmaIdentity Identity) ReadResponse(RemoteUseCaseDescriptor descriptor,
            DataPortalResponse response, ServerSettings settings, CarmaIdentity identity)
        {
            using (response)
            {
                if (response.Kind == DataPortalResultKind.Error)
                {
                    var error = response.Error ?? new RemoteError();

                    // a call the server cannot read usually means that the server has been updated:
                    // the next call fetches the settings again and reports the mismatch
                    if (error.Kind == RemoteErrorKind.BadRequest || error.Kind == RemoteErrorKind.VersionMismatch)
                        _settingsProvider.Invalidate(_connection.Endpoint);

                    // refused for its plugins: the same exception, with its data, as a mismatch found before a call
                    if (error.Kind == RemoteErrorKind.VersionMismatch)
                        throw VersionMismatch(null, "the server has other plugins installed");

                    throw ToException(error, true);
                }

                object result = null;

                switch (response.Kind)
                {
                    case DataPortalResultKind.None:
                        if (null != descriptor.Result && !descriptor.Result.AllowsNull) throw Mismatch();
                        break;

                    case DataPortalResultKind.File:
                        if (null == descriptor.Result || !descriptor.Result.IsFile) throw Mismatch();
                        result = response.TakeFile() ?? throw Mismatch();
                        break;

                    case DataPortalResultKind.Value:
                        if (null == descriptor.Result || descriptor.Result.IsFile
                            || response.ResultType != descriptor.Result.WireTypeName) throw Mismatch();
                        result = ReadValue(descriptor.Result, response.Value, settings.MaxDepth);
                        break;

                    default:
                        throw Mismatch();
                }

                CarmaIdentity updated;
                try
                {
                    updated = IdentityUpdate.Apply(response.IdentityUpdate, identity, _registry.IdentityType);
                }
                catch (JsonException)
                {
                    (result as IDisposable)?.Dispose();
                    throw Mismatch();
                }

                return (result, updated);
            }
        }

        private object ReadValue(RemoteValueDescriptor descriptor, ReadOnlyMemory<byte> value, int maxDepth)
        {
            // an empty value is not valid JSON either, but this says what is wrong without parsing
            if (value.IsEmpty) throw Mismatch();

            try
            {
                var serializer = DataPortalSerialization.CreateSerializer(maxDepth);
                serializer.Context = new StreamingContext(StreamingContextStates.Remoting,
                    new NoFiles(_registry.TrustedWireTypes));

                using (var textReader = new StreamReader(value.OpenRead(), Utf8))
                using (var jsonReader = new JsonTextReader(textReader) { MaxDepth = maxDepth })
                {
                    if (!jsonReader.Read()) throw Mismatch();

                    var result = descriptor.Read(jsonReader, serializer, _context);

                    if (jsonReader.Read()) throw Mismatch();

                    return result;
                }
            }
            catch (RemoteCallRejectedException)
            {
                // the server has answered with something this client does not expect:
                // most likely the two are of different versions
                throw Mismatch();
            }
            catch (JsonException)
            {
                throw Mismatch();
            }
        }

        private Exception Mismatch()
        {
            // most likely the server has been updated since the settings were fetched
            _settingsProvider.Invalidate(_connection.Endpoint);
            return ToException(new RemoteError { Kind = RemoteErrorKind.BadRequest }, false);
        }

        private Exception LimitExceeded(string violation)
            => ToException(new RemoteError { Kind = RemoteErrorKind.BadRequest }, false,
                new InvalidOperationException($"The call exceeds the data portal limits: {violation}."));

        /// <summary>
        /// Converts a transport failure to the exception to throw: a <see cref="DataPortalUnavailableException"/>
        /// if the server is unavailable, otherwise a fault found by the client (not logged yet).
        /// </summary>
        private Exception ToException(DataPortalTransportException exception)
            => exception.Kind == RemoteErrorKind.ServiceUnavailable
                ? new DataPortalUnavailableException(exception)
                : ToException(new RemoteError { Kind = exception.Kind }, false, exception);

        /// <summary>
        /// Re-creates the exception of a failure by its class: a <see cref="BusinessException"/> with the server's
        /// message, a <see cref="NotAuthenticatedException"/>, or a <see cref="RemoteCallException"/> for a fault.
        /// </summary>
        /// <param name="error">the error</param>
        /// <param name="reportedByServer">whether the server has reported (and logged) it</param>
        /// <param name="innerException">the exception that caused it on the client; null for none</param>
        private static Exception ToException(RemoteError error, bool reportedByServer, Exception innerException = null)
        {
            Exception result;
            switch (error.Kind)
            {
                case RemoteErrorKind.Business:
                    result = new BusinessException(error.Message ?? string.Empty);
                    break;
                case RemoteErrorKind.Unauthenticated:
                    result = new NotAuthenticatedException();
                    break;
                default:
                    result = new RemoteCallException(error, innerException);
                    break;
            }

            return reportedByServer ? result.MarkLogged(error.CorrelationId) : result;
        }

        #endregion
    }
}
