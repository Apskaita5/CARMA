# 6. Troubleshooting

Symptom → likely cause → fix. Messages are quoted as the framework writes them (abbreviated).

## Startup (`IoCManagerBase.Initialize`, container build)

| Symptom | Cause | Fix |
|---|---|---|
| `The build configuration ... is not an app build` | `Any`, `Backend`, `Local` or 0 passed to the IoC manager | use `Server`, `Client`, `Client \| Server` or `Client \| Local` |
| `No app assemblies to init.` | `AppAssemblies` empty or only framework assemblies | list the app's assemblies |
| `... declares that it implements interface ... however this interface does not have a ServiceAttribute.` | `[Service]` missing on the interface | add `[Service(...)]` |
| `Service ... has more than one default implementation: A and B.` | two app implementations in one build | narrow their `BuildConfiguration`s, or move one to another assembly (e.g. thin-client stubs) |
| `The use cases/types A and B have the same name, so the data portal cannot tell them apart.` | same full name in two assemblies | rename/move one |
| `Assembly ... cannot be scanned, some of its types cannot be loaded: ...` | a missing dependency of an app assembly | deploy the dependency; the message lists the loader errors |
| `Unmanaged assembly ... was not found.` | native library missing for this platform | deploy `.dll`/`.so`/`.dylib`; check `ResolveUnmanagedPath` |
| `Failed to instantiate ConfigurationBase implementation of type ...` | configuration class without a public parameterless constructor | add one |
| `The DataPortalServerOptions configuration is not valid: ...` (or another options class) | a setting out of range; spool directory missing | fix the setting ([05](05-configuration-reference.md)) |
| `Plugin folder ... does not contain the plugin library.` | library name differs from the folder name / `LibraryName` | rename, or set `PluginsConfiguration:LibraryName` |
| `Plugin ... has no PluginId attribute` | `[assembly: PluginId]` missing | `Directory.Build.props` ([02 §9.2](02-ioc-manager.md#92-plugin-projects)) |
| `Plugins A and B have the same identifier ...` | two folders with the same plugin | remove one |
| `Plugin ... is refused: ...` | conventions, name clash, conflicting override | read the listed violations |
| `The plugins folder contains plugins: override LoadPlugin to load them.` | `LoadPlugin` not implemented | implement it ([02 §9.3](02-ioc-manager.md#93-loading)) |
| `The app has tenant scoped services: override AddServiceFactory ...` | `[TenantScoped]` used, factory not implemented | implement `AddServiceFactory` |
| `Service ... is a singleton and cannot be tenant scoped` | `[TenantScoped]` on a singleton | make it scoped/transient, or remove `[TenantScoped]` |
| `... an app scoped service cannot depend on the tenant scoped ...` | constructor dependency | make the dependent tenant scoped too, or resolve lazily inside a tenant scope |
| `... is a system service for ..., which ... already implements` | `AddSystemServices` for an interface something else implements | remove the other implementation or the system service |
| `The HTTP data portal handler is already set; it can be set only once per process.` | second IoC manager returns a handler | return `null` from the second |
| `ValidateOnBuild`: `Unable to resolve service for type 'IGreetingRepository' while attempting to activate 'GreetUseCase'` in a thin client | use case registered everywhere, repository `Backend` only | stubs assembly for the thin client |
| `ValidateOnBuild`: `IAppVersionProvider` / `IRemoteErrorMapper` / `ILogger` / `IAuthenticationStateProvider` not resolvable | app service missing | implement it ([02 §12](02-ioc-manager.md#12-services-every-app-must-provide)) |
| `IAppVersionProvider must give both the name and the version` (server start) | empty name / null version | fix the provider |

## Connecting

| Symptom | Cause | Fix |
|---|---|---|
| `No data portal endpoint is set for this scope, and none is configured.` | thin client scope without `Initialize` and without configuration | initialize the connection or configure `DataPortalConnectionConfiguration` |
| `Unknown data portal transport local; available: http.` | local endpoint in a thin client | thin clients are remote only |
| `Data portal transport http needs a server address.` / `takes no server address` | endpoint address missing / given for local | fix the endpoint |
| `The data portal endpoint of a scope cannot change: create a new scope for another server.` | `Initialize` after the connection was read, or twice | create a new scope; initialize first |
| `VersionMismatch`: "the server runs application 'X', the client is 'Y'" | different `AppName` | same name on both |
| `VersionMismatch`: "the server runs version ..., the client is version ..." | versions differ beyond `AppVersionMatch` | update the client; or relax `AppVersionMatch` |
| `VersionMismatch`: "different plugins installed" | plugin fingerprints differ | deploy the same plugins |
| `VersionMismatch`: "speaks data portal protocol N" | framework protocol version differs | update the older side |
| `ServiceUnavailable`: "could not be reached" / "did not answer within ..." | server down, DNS, TLS, timeout | check connectivity; raise `CallTimeout` for long calls |
| `ServerError`: "answered with HTTP status 404 without a data portal result" | wrong address (route, path base) | absolute URL of the mapped route |
| `ServerError`: status 401/403 without a result | an authorization policy on the endpoint | remove it ([04 §7](04-aspnetcore-endpoint.md#7-authentication-authorization-and-other-middleware)) |
| `ServerError` on every call from a browser, server logs show success | CORS exposed headers missing | expose `DataPortalHeaders.ResponseHeaders` |
| `ServerError`: status 413 / 404.13 | proxy or IIS body limit | raise it ([04 §8](04-aspnetcore-endpoint.md#8-kestrel-iis-and-reverse-proxies)) |
| `ServiceUnavailable`: status 502/504 on large uploads | proxy timeout or buffering | proxy timeouts, `proxy_request_buffering off` |

## Calls

| Symptom | Cause | Fix |
|---|---|---|
| `The identity has no carma:token claim to authenticate the call with.` | unauthenticated identity passed | pass `null` for anonymous calls; check `TokenClaimType` |
| `The token must consist of printable ASCII characters without spaces` | token format | use a JWT or encode the token |
| `Unauthorized` for every call | token not verified (expired, other server's key) or anonymous call refused | log in again; check `VerifyIdentityAsync` |
| `Unauthorized`: "The tenant's plugins have changed: enter the tenant again." | a plugin in the identity is no longer installed | re-enter the tenant (new identity) |
| `Unauthorized`: "Use case ... requires a tenant account." | tenant scoped app use case called without a tenant | enter a tenant first |
| `BadRequest` for one use case only (server log: "use case '...' is unknown") | interface not on the server, renamed, or plugin use case for a tenant without the plugin | deploy/rename consistently |
| `BadRequest` (server log: "parameter N of ... should be ...") | client and server signatures differ | same contracts on both sides |
| `BadRequest` before sending: "The call exceeds the data portal limits: ..." | payload/file/upload limits | raise server limits or send less |
| `NotSupportedException`: "... is not a trusted wire type" | `Type` value or generic argument not trusted | only lookups/entities, domain objects, use case implementations |
| `NotSupportedException`: "exactly one [RemoteMethod] is required" | contract error | fix the interface; the convention test finds these |
| `InvalidOperationException`: "The serialization constructor of ... must be private." / "... has parameter(s) ... with no serialized property of the same name and type." | read-only value contract | private constructor, matching names and types |
| `JsonSerializationException`: "Dictionary type ... has key type ..." | unsafe dictionary key | string/enum/primitive-like keys |
| `ServerError` with a correlation id | unmapped exception on the server | find the id in the server log; map business exceptions |
| `InvalidOperationException`: "... is tenant scoped, and the current scope has no tenant." | resolving a tenant scoped service without a tenant | initialize `ITenantContext` with a tenant |
| `InvalidOperationException`: "The tenant of this scope is not set yet" | tenant context read before `Initialize` | initialize it when the scope is created |
| Uploaded file disappeared after the use case returned | spooled files are deleted with the response | copy the file during the call |
| `IOException` while reading a download | server aborted the stream mid-way | retry; check the server log |
| HTTP connections exhausted on a client | downloaded `FileContent` not disposed | `using` |
| Permissions granted mid-session not effective | refreshed identity ignored | apply the identity returned by the portal |
