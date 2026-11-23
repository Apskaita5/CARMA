using A5Soft.CARMA.Application.DataPortal.Internal;
using A5Soft.CARMA.Application.DataPortal.Protocol;
using A5Soft.CARMA.Application.IoC.Plugins;
using A5Soft.CARMA.Application.Tenancy;
using A5Soft.CARMA.Domain.Files;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal.Server
{
    /// <summary>
    /// The server side of the data portal, in the canonical, transport independent form. A server
    /// endpoint translates what its transport has received into a <see cref="DataPortalCall"/> and an
    /// <see cref="IDataPortalRequestBody"/>, and the resulting <see cref="DataPortalResponse"/> back
    /// into its transport. The call is cleared step by step, using the call description only, and only
    /// then is its data read:
    /// <list type="number">
    /// <item>the caller's culture is applied;</item>
    /// <item>the token is verified, which yields the caller's identity;</item>
    /// <item>the use case is found in the registry and resolved from the request scope;</item>
    /// <item>the caller is authorized;</item>
    /// <item>the parameter types and the file count are checked;</item>
    /// <item>the payload is read, and every file is spooled to a temp file, within the limits;</item>
    /// <item>the parameters are deserialized: domain objects are rebuilt from their DTOs, and the files
    /// are reattached wherever they were referenced;</item>
    /// <item>the use case is invoked, and the result is returned as JSON or as a file.</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// <para>The host derives from this class to supply what only it knows: how a token is verified
    /// and how a use case is authorized.</para>
    /// <para>The use case receives the uploaded files only once all of them have arrived and passed
    /// the checks, as seekable streams of known length. It never sees a partial upload. The files are
    /// deleted when the response is disposed, so a use case that keeps a file must copy it during the
    /// call.</para>
    /// </remarks>
    public abstract class ServerDataPortalBase
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        private readonly RemoteUseCaseRegistry _registry;
        private readonly IRemoteErrorMapper _errorMapper;
        private readonly IDataPortalServerOptions _options;
        private readonly IAppVersionProvider _versionProvider;
        private readonly TenantServiceMap _tenantServices;
        private readonly PluginCatalog _plugins;


        /// <summary>
        /// Creates a new server data portal.
        /// </summary>
        /// <param name="registry">a registry of the remote use cases</param>
        /// <param name="tenantServices">the tenant scoped services, and the rules for tenant plugin assignments</param>
        /// <param name="plugins">the plugins installed on the server, compared with each caller's</param>
        /// <param name="errorMapper">a mapper of application exceptions to remote errors</param>
        /// <param name="options">the server data portal options</param>
        /// <param name="versionProvider">the identity of the application, published to the clients</param>
        /// <param name="logger">a logger; null for none</param>
        /// <remarks>The <see cref="IApplicationContext"/> is resolved from the request scope for
        /// each call, as its services (e.g. the validation provider) may be scoped.</remarks>
        /// <exception cref="InvalidOperationException">the version provider gives no name or version</exception>
        protected ServerDataPortalBase(RemoteUseCaseRegistry registry, TenantServiceMap tenantServices,
            PluginCatalog plugins, IRemoteErrorMapper errorMapper, IDataPortalServerOptions options,
            IAppVersionProvider versionProvider, ILogger logger)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _tenantServices = tenantServices ?? throw new ArgumentNullException(nameof(tenantServices));
            _plugins = plugins ?? throw new ArgumentNullException(nameof(plugins));
            _errorMapper = errorMapper ?? throw new ArgumentNullException(nameof(errorMapper));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _versionProvider = versionProvider ?? throw new ArgumentNullException(nameof(versionProvider));
            Logger = logger;

            // fail at startup rather than publish settings no client accepts
            if (string.IsNullOrWhiteSpace(versionProvider.AppName) || null == versionProvider.AppVersion)
                throw new InvalidOperationException(
                    $"{nameof(IAppVersionProvider)} must give both the name and the version of the application.");
        }


        /// <summary>Gets a logger.</summary>
        protected ILogger Logger { get; }

        /// <summary>Gets the server data portal options.</summary>
        public IDataPortalServerOptions Options => _options;


        /// <summary>
        /// Gets the public settings of the server, for an endpoint to publish to the clients: the identity
        /// of the application and the limits every call must respect. Public to anyone who can reach the
        /// endpoint, without authentication.
        /// </summary>
        /// <returns>a new instance, which the caller may change</returns>
        public ServerSettings GetServerSettings() => ServerSettings.Create(_options, _versionProvider, _plugins);


        /// <summary>
        /// Handles a call. Never throws: a failure is returned as an error response.
        /// </summary>
        /// <param name="call">the description of the call, as received; untrusted</param>
        /// <param name="body">the data of the call, read only once the call has been cleared</param>
        /// <param name="services">a service provider of the current request scope</param>
        /// <param name="cancellationToken">a token signalled when the caller goes away</param>
        /// <returns>a response for the endpoint to send; the endpoint disposes it once it is sent</returns>
        public async Task<DataPortalResponse> HandleAsync(DataPortalCall call, IDataPortalRequestBody body,
            IServiceProvider services, CancellationToken cancellationToken = default)
        {
            var state = new CallState();
            try
            {
                if (null == call) throw new ArgumentNullException(nameof(call));
                if (null == body) throw new ArgumentNullException(nameof(body));
                if (null == services) throw new ArgumentNullException(nameof(services));

                // culture first, so that even a refusal is localized for the caller; the method is
                // async, so the culture does not leak to the endpoint
                ApplyCulture(call);

                return await ExecuteAsync(call, body, services, state, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                foreach (var resource in state.Resources) resource.Dispose();

                return CreateErrorResponse(ex, !state.BodyStarted);
            }
        }

        /// <summary>
        /// Creates an error response for a failure, mapped, logged and stripped the same way as the
        /// failures of <see cref="HandleAsync"/>. For an endpoint that refuses a call itself, before
        /// handing it over (e.g. a malformed transport message).
        /// </summary>
        /// <param name="exception">the failure; a <see cref="RemoteCallRejectedException"/> for a refusal</param>
        /// <param name="refusedBeforeBody">whether the call was refused before any of its data was read</param>
        public DataPortalResponse CreateErrorResponse(Exception exception, bool refusedBeforeBody)
            => DataPortalResponse.ForError(ToError(exception), refusedBeforeBody);


        #region Host supplied

        /// <summary>
        /// Verifies the caller's token and gets the caller's identity, and makes it the current
        /// identity of the request scope, so that the use case sees it.
        /// </summary>
        /// <param name="token">the token received with the call; untrusted</param>
        /// <param name="services">a service provider of the current request scope</param>
        /// <returns>the caller's identity; null if the token is invalid or has expired</returns>
        protected abstract Task<ClaimsIdentity> VerifyIdentityAsync(string token, IServiceProvider services);

        /// <summary>
        /// Authorizes the call. Invoked for every call, before any of its data is read.
        /// </summary>
        /// <param name="useCase">a use case instance resolved from the request scope</param>
        /// <param name="descriptor">a descriptor of the use case</param>
        /// <param name="identity">a verified identity of the caller; null for an anonymous call</param>
        /// <param name="services">a service provider of the current request scope</param>
        /// <remarks>Must throw if the caller is not allowed to invoke the use case; an anonymous call
        /// must be refused unless the use case is an unauthenticated one. The exception is reported to
        /// the client through the error mapper.</remarks>
        protected abstract Task AuthorizeAsync(object useCase, RemoteUseCaseDescriptor descriptor,
            ClaimsIdentity identity, IServiceProvider services);

        /// <summary>
        /// Gets a refreshed identity to send back to the client; null if it has not changed.
        /// </summary>
        /// <param name="identity">a verified identity of the caller; null for an anonymous call</param>
        /// <param name="services">a service provider of the current request scope</param>
        protected virtual Task<ClaimsIdentity> GetRefreshedIdentityAsync(ClaimsIdentity identity,
            IServiceProvider services)
            => Task.FromResult<ClaimsIdentity>(null);

        /// <summary>
        /// Gets the tenant of a call from the caller's verified identity, e.g. from a tenant claim. Default: no
        /// tenant, for an app that is not tenant based.
        /// </summary>
        /// <param name="identity">a verified identity of the caller; null for an anonymous call</param>
        protected virtual string GetTenantId(ClaimsIdentity identity) => null;

        /// <summary>
        /// Gets the identifiers of the plugins assigned to the call's tenant from the caller's verified identity,
        /// e.g. from plugin claims put there (after <see cref="TenantServiceMap.ValidateAssignment"/>) when the
        /// identity was issued. Called only for a call with a tenant. Default: none.
        /// </summary>
        /// <param name="identity">a verified identity of the caller</param>
        protected virtual IReadOnlyCollection<string> GetTenantPlugins(ClaimsIdentity identity)
            => Array.Empty<string>();

        #endregion

        #region Pipeline

        /// <summary>
        /// What is known about a call as it proceeds, for the error response.
        /// </summary>
        private sealed class CallState
        {
            public readonly List<IDisposable> Resources = new List<IDisposable>();
            public bool BodyStarted;
        }

        private async Task<DataPortalResponse> ExecuteAsync(DataPortalCall call, IDataPortalRequestBody body,
            IServiceProvider services, CallState state, CancellationToken cancellationToken)
        {
            // plugins: a client with other plugins is refused before anything else is read
            if (!string.Equals(call.PluginFingerprint ?? string.Empty, _plugins.Fingerprint ?? string.Empty, StringComparison.Ordinal))
                throw new RemoteCallRejectedException(RemoteErrorKind.VersionMismatch,
                    "The client and the server have different plugins installed.");

            // identity
            ClaimsIdentity identity = null;
            if (null != call.Token)
            {
                identity = await VerifyIdentityAsync(call.Token, services).ConfigureAwait(false);

                if (null == identity) throw new RemoteCallRejectedException(RemoteErrorKind.Unauthorized,
                    "The token is invalid or has expired.");
            }

            // use case: looked up in the registry, never resolved by name
            RemoteUseCaseDescriptor descriptor;
            try
            {
                if (!_registry.TryGet(call.UseCase, out descriptor))
                    throw Rejected($"use case '{Truncate(call.UseCase)}' is unknown");
            }
            catch (NotSupportedException ex)
            {
                // registered, but cannot be described: a defect of the server, not of the call
                throw new InvalidOperationException(ex.Message, ex);
            }

            // generic arguments: resolved through the trusted wire types only
            if (descriptor.IsGenericDefinition || call.GenericArguments.Count > 0)
            {
                try
                {
                    descriptor = descriptor.CloseByNames(call.GenericArguments);
                }
                catch (NotSupportedException ex)
                {
                    throw Rejected(ex.Message);
                }
            }

            // tenant: the tenant and its plugins, fixed for the call's scope before anything tenant scoped is
            // resolved
            var tenantContext = (ITenantContext)services.GetService(typeof(ITenantContext))
                ?? throw new InvalidOperationException($"{nameof(ITenantContext)} is not registered in the container.");
            var tenantId = GetTenantId(identity);
            var tenantPlugins = null == tenantId
                ? Array.Empty<string>()
                : GetTenantPlugins(identity) ?? Array.Empty<string>();

            // an identity issued before a plugin was removed from the server
            if (!_plugins.AreInstalled(tenantPlugins)) throw new RemoteCallRejectedException(
                RemoteErrorKind.Unauthorized, "The tenant's plugins have changed: enter the tenant again.");
            tenantContext.Initialize(tenantId, tenantPlugins);

            if (!_tenantServices.IsAvailable(descriptor.InterfaceType, tenantContext))
            {
                // an app use case needing a tenant may say so; a plugin's use case is answered exactly like an
                // unknown name, so that nothing tells another tenant's plugins apart
                if (null == tenantId && _tenantServices.HasDefault(descriptor.InterfaceType))
                    throw new RemoteCallRejectedException(RemoteErrorKind.Unauthorized,
                        $"Use case {descriptor.Name} requires a tenant account.");

                throw Rejected($"use case '{Truncate(call.UseCase)}' is unknown");
            }

            var useCase = services.GetService(descriptor.InterfaceType)
                ?? throw new InvalidOperationException(
                    $"Use case {descriptor.Name} is not registered in the container.");

            // the domain context is all the domain objects need (the framework registers a default)
            var context = (Domain.IApplicationContext)services.GetService(typeof(Domain.IApplicationContext))
                ?? throw new InvalidOperationException(
                    $"{nameof(Domain.IApplicationContext)} is not registered in the container.");

            // authorization, before any of the data is read
            await AuthorizeAsync(useCase, descriptor, identity, services)
                .ConfigureAwait(false);

            // the call against the signature and the limits, still from the description only
            if (call.ParameterTypes.Count != descriptor.Parameters.Count) throw Rejected(
                $"use case {descriptor.Name} takes {descriptor.Parameters.Count} parameter(s), " +
                $"{call.ParameterTypes.Count} received");

            for (var i = 0; i < call.ParameterTypes.Count; i++)
            {
                if (!string.Equals(call.ParameterTypes[i], descriptor.Parameters[i].WireTypeName, StringComparison.Ordinal))
                    throw Rejected($"parameter {i} of {descriptor.Name} should be {descriptor.Parameters[i].WireTypeName}");
            }

            if (call.FileCount > _options.MaxFileCount)
                throw Rejected($"{call.FileCount} files declared, more than {_options.MaxFileCount}");

            // the data
            state.BodyStarted = true;

            var payload = await body.ReadPayloadAsync(_options.MaxPayloadLength, cancellationToken)
                .ConfigureAwait(false);
            // an empty payload is refused while it is parsed, as it is not a JSON array
            if (payload.Length > _options.MaxPayloadLength)
                throw Rejected($"the payload is longer than {_options.MaxPayloadLength}");

            var files = await SpoolFilesAsync(body, call.FileCount, state, cancellationToken)
                .ConfigureAwait(false);

            if (null != await body.ReadNextFileAsync(cancellationToken).ConfigureAwait(false))
                throw Rejected($"the call carries more than the {call.FileCount} declared files");

            var args = ReadParameters(payload, descriptor, files, context);

            // invocation
            var result = await InvokeAsync(descriptor, useCase, args).ConfigureAwait(false);

            var refreshedIdentity = await GetRefreshedIdentityAsync(identity, services).ConfigureAwait(false);

            return CreateResultResponse(descriptor, result, refreshedIdentity, state.Resources);
        }

        /// <summary>
        /// Spools every file to a temp file, so that the use case gets seekable streams of known
        /// length, and only once the whole call has arrived and passed the checks.
        /// </summary>
        private async Task<List<FileContent>> SpoolFilesAsync(IDataPortalRequestBody body, int fileCount,
            CallState state, CancellationToken cancellationToken)
        {
            var files = new List<FileContent>(fileCount);
            var buffer = new byte[_options.CopyBufferSize];
            long total = 0;

            for (var i = 0; i < fileCount; i++)
            {
                var incoming = await body.ReadNextFileAsync(cancellationToken).ConfigureAwait(false)
                    ?? throw Rejected($"the call carries {i} of the {fileCount} declared files");

                var spool = FileSpool.Create(_options.SpoolDirectory, _options.CopyBufferSize);
                var file = new FileContent(spool, Sanitizer.SanitizeContentType(incoming.ContentType),
                    Sanitizer.SanitizeFileName(incoming.FileName, "upload"));
                state.Resources.Add(file);

                long length = 0;
                int read;
                while ((read = await incoming.Content.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                    .ConfigureAwait(false)) > 0)
                {
                    length += read;
                    total += read;

                    if (length > _options.MaxFileLength || total > _options.MaxUploadLength) throw Rejected(
                        $"file {i} is over the limit ({_options.MaxFileLength} per file, {_options.MaxUploadLength} in total)");

                    await spool.WriteAsync(buffer, 0, read, cancellationToken)
                        .ConfigureAwait(false);
                }

                await spool.FlushAsync(cancellationToken)
                    .ConfigureAwait(false);
                spool.Position = 0;

                files.Add(file);
            }

            return files;
        }

        private object[] ReadParameters(ReadOnlyMemory<byte> payload, RemoteUseCaseDescriptor descriptor,
            IReadOnlyList<FileContent> files, Domain.IApplicationContext context)
        {
            var args = new object[descriptor.Parameters.Count];

            var incoming = new IncomingFiles(files, _registry.TrustedWireTypes);
            var serializer = DataPortalSerialization.CreateSerializer(_options.MaxDepth);
            serializer.Context = new StreamingContext(StreamingContextStates.Remoting, incoming);

            using (var textReader = new StreamReader(payload.OpenRead(), Utf8))
            using (var jsonReader = new JsonTextReader(textReader) { MaxDepth = _options.MaxDepth })
            {
                try
                {
                    if (!jsonReader.Read() || jsonReader.TokenType != JsonToken.StartArray)
                        throw Rejected("the payload is not an array");

                    for (var i = 0; i < args.Length; i++)
                    {
                        if (!jsonReader.Read() || jsonReader.TokenType == JsonToken.EndArray)
                            throw Rejected("the payload has fewer values than parameters");

                        args[i] = descriptor.Parameters[i].Read(jsonReader, serializer, context);
                    }

                    if (!jsonReader.Read() || jsonReader.TokenType != JsonToken.EndArray)
                        throw Rejected("the payload has more values than parameters");

                    if (jsonReader.Read()) throw Rejected("the payload continues past the array");
                }
                catch (JsonException ex)
                {
                    throw Rejected("the payload is not valid JSON", ex);
                }
            }

            if (!incoming.AllClaimed) throw Rejected("a file was sent that no parameter references");

            return args;
        }

        private static async Task<object> InvokeAsync(RemoteUseCaseDescriptor descriptor,
            object useCase, object[] args)
        {
            Task task;
            try
            {
                task = (Task)descriptor.Method.Invoke(useCase, args);
            }
            catch (TargetInvocationException ex) when (null != ex.InnerException)
            {
                // the use case has thrown before its first await
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }

            if (null == task) throw new InvalidOperationException(
                $"Remote method {descriptor.Name}.{descriptor.Method.Name} has returned a null task.");

            await task.ConfigureAwait(false);

            return descriptor.GetResult(task);
        }

        private DataPortalResponse CreateResultResponse(RemoteUseCaseDescriptor descriptor,
            object result, ClaimsIdentity refreshedIdentity, List<IDisposable> resources)
        {
            var identity = null == refreshedIdentity ? null : IdentityEnvelope.From(refreshedIdentity);

            if (null == descriptor.Result || null == result) return DataPortalResponse.ForNone(identity, resources);

            if (descriptor.Result.IsFile) return DataPortalResponse.ForFile((FileContent)result, identity, resources);

            // serialized here, so that a failure is still reported as an error; files nested in the
            // result are written as null
            var json = new MemoryStream();
            resources.Add(json);
            using (var textWriter = new StreamWriter(json, Utf8, 4096, true))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                var serializer = DataPortalSerialization.CreateSerializer(_options.MaxDepth);
                serializer.Context = new StreamingContext(StreamingContextStates.Remoting, new ResponseFiles(_registry.TrustedWireTypes));
                descriptor.Result.Write(jsonWriter, serializer, result);
            }

            // over the stream's own buffer, no copy; the stream is among the response's resources,
            // so the memory stays valid until the endpoint has sent the response and disposed it
            return DataPortalResponse.ForValue(descriptor.Result.WireTypeName, json.WrittenBytes(), identity, resources);
        }

        #endregion

        #region Helpers

        private static void ApplyCulture(DataPortalCall call)
        {
            var culture = GetCulture(call.Culture);
            if (null != culture) CultureInfo.CurrentCulture = culture;

            var uiCulture = GetCulture(call.UICulture);
            if (null != uiCulture) CultureInfo.CurrentUICulture = uiCulture;
        }

        private static CultureInfo GetCulture(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 32) return null;

            try
            {
                return CultureInfo.GetCultureInfo(name);
            }
            catch (CultureNotFoundException)
            {
                return null;
            }
        }

        private static string Truncate(string value)
            => null == value || value.Length <= 200 ? value : value.Substring(0, 200);

        #endregion

        #region Errors

        private RemoteError ToError(Exception exception)
        {
            RemoteError result = null;

            if (exception is RemoteCallRejectedException rejected)
            {
                result = new RemoteError { Kind = rejected.Kind };
            }
            else
            {
                try
                {
                    result = _errorMapper.ToError(exception);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex);
                }
            }

            if (null == result) result = new RemoteError { Kind = RemoteErrorKind.ServerError };

            switch (result.Kind)
            {
                case RemoteErrorKind.Business:
                case RemoteErrorKind.Validation:
                case RemoteErrorKind.Concurrency:
                    // the user's to resolve; the use case has logged it already
                    break;

                case RemoteErrorKind.ServerError:
                case RemoteErrorKind.BadRequest:
                    // nothing about the cause leaves the server
                    result.Message = null;
                    result.Data = null;
                    LogWithCorrelationId(result, exception);
                    break;

                default:
                    LogWithCorrelationId(result, exception);
                    break;
            }

            return result;
        }

        private void LogWithCorrelationId(RemoteError error, Exception exception)
        {
            error.CorrelationId = Guid.NewGuid().ToString("N");

            Logger?.LogError(new InvalidOperationException(
                $"Remote call has failed ({error.Kind}), correlation id {error.CorrelationId}.",
                exception));
        }

        private static RemoteCallRejectedException Rejected(string reason, Exception inner = null)
            => new RemoteCallRejectedException(RemoteErrorKind.BadRequest,
                $"Remote call was rejected: {reason}.", inner);

        #endregion
    }
}
