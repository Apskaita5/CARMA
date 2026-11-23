# 3. Implementing the app's data portal components

This guide lists what an app writes for the data portal and shows a working implementation of each piece. The
mechanics behind them are in [01](01-data-portal-mechanics.md); registration and deployments in
[02](02-ioc-manager.md).

**Contents**

1. [Checklist](#1-checklist)
2. [Declaring remote use cases](#2-declaring-remote-use-cases)
3. [Values that cross the data portal](#3-values-that-cross-the-data-portal)
4. [App-level use case bases](#4-app-level-use-case-bases)
5. [Lookups](#5-lookups)
6. [The version provider](#6-the-version-provider)
7. [The remote error mapper](#7-the-remote-error-mapper)
8. [The authentication state provider and the token](#8-the-authentication-state-provider-and-the-token)
9. [The server data portal](#9-the-server-data-portal)
10. [Client sessions: connection, identity, tenant](#10-client-sessions-connection-identity-tenant)
11. [Handling data portal errors in the UI](#11-handling-data-portal-errors-in-the-ui)
12. [A custom transport](#12-a-custom-transport)
13. [Pitfalls](#13-pitfalls)

---

## 1. Checklist

| Component | Where | Builds | Section |
|---|---|---|---|
| Remote use case interfaces | contracts assembly | all | §2 |
| Use case implementations, deriving from app-level bases | app assembly | all (`Any`) | §4 |
| Repositories | backend assembly | `Backend` | [02 §3](02-ioc-manager.md#3-choosing-the-build-configuration-of-an-implementation) |
| Repository stubs | stubs assembly | thin client only | [02 §3](02-ioc-manager.md#3-choosing-the-build-configuration-of-an-implementation) |
| `IAppVersionProvider` | shared | all | §6 |
| `IRemoteErrorMapper` and the app's exceptions | shared | all | §7 |
| `IAuthenticationStateProvider` | shared or per host | all | §8 |
| `ILogger` adapter | shared | all | §8.3 |
| Server data portal class | server host | `Server` | §9 |
| Token issuing and verification | server | `Server` | §8, §9 |
| Session handling (scope per login) | each client | client builds | §10 |
| Convention unit test | test project | — | §3.4 |

---

## 2. Declaring remote use cases

```csharp
using A5Soft.CARMA.Application;

[RemoteService]                    // or [UseCase(lifetime, requiredLookups...)] for use cases
public interface IGreetUseCase
{
    [RemoteMethod]
    Task<Greeting> ExecuteAsync(string name);
}
```

- **One `[RemoteMethod]`** per interface (counting the interfaces it extends), returning `Task` or `Task<T>`.
- **One remote interface per implementation.** The client finds the interface from the implementation type; two
  remote interfaces on one class fail (*"implements more than one remote use case interface"*). An interface that
  only extends another remote interface does not count as a second one.
- **Parameters**: up to three (the `IClientDataPortal` overloads take 0–3 arguments); put more into a criteria
  class.
- **Wire name** = `Type.ToString()` of the interface: renaming or moving the interface is a breaking protocol change
  (old clients get "unknown use case" → `BadRequest`).
- The interface must have an implementation marked `[DefaultServiceImplementation]` in a scanned assembly, or it
  is not in the registry.
- **Tenant scoped** use case: add `[TenantScoped]` to the interface ([02 §10](02-ioc-manager.md#10-tenancy)).

---

## 3. Values that cross the data portal

### 3.1 Plain values

A class with a public parameterless constructor (or a single public constructor) and typed members:

```csharp
public sealed class Greeting
{
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 3.2 Read-only values: the serialization constructor

```csharp
using A5Soft.CARMA.Domain;

public sealed class InvoiceSummary
{
    [SerializationConstructor]
    private InvoiceSummary(string number, decimal total)      // private: only the serializer builds it from raw values
    {
        Number = number;
        Total = total;
    }

    public static InvoiceSummary Create(string number, decimal total) => new InvoiceSummary(number, total);

    public string Number { get; }
    public decimal Total { get; }
}
```

- The constructor **must be private** and there must be **only one** marked.
- **Every parameter matches a serialized property** with the same name (ignoring case) and the **same type**
  (`decimal total` ↔ `decimal Total`; `int` ↔ `long` does not match). A mismatch fails on first use rather than
  leaving the parameter at its default.
- Lookups (`LookupBase<T>`) and query results (`QueryResultBase`, `QueryResultItemBase`) **require** one. A query
  result's subtotals are recalculated after deserialization; do not take them in the constructor.

### 3.3 Domain objects, files, types, collections

| You want to pass | Do | Do not |
|---|---|---|
| A domain object | as a top-level parameter or the result; it travels as its DTO and is rebuilt on arrival | nest it in a POCO, DTO or list |
| A file upload | a `FileContent` parameter, or a `FileContent` property of a parameter (any depth) | a `Stream` or `byte[]` of a large file |
| A file download | `Task<FileContent>` as the result | a file nested in a result (arrives `null`) |
| A type | a `Type` value that is a lookup type, its entity, a domain object or a use case implementation | any other type (refused) |
| A dictionary | keys: string, enum, primitive, `Guid`, `DateTime(Offset)`, `TimeSpan`, `decimal` | `Type` or class keys |
| A collection | `List<T>`, `T[]`, `IReadOnlyList<T>`, `IEnumerable<T>`, `IReadOnlyDictionary<K,V>` | `ArrayList`, `object`, `JToken`, custom interfaces |

Upload and download use cases (both verified over HTTP):

```csharp
using A5Soft.CARMA.Domain.Files;

public sealed class ImportRequest
{
    public string Comment { get; set; }
    public FileContent File { get; set; }        // sent as a multipart part, referenced from the JSON
}

[RemoteService]
public interface IImportInvoicesUseCase
{
    [RemoteMethod]
    Task<InvoiceSummary> ImportAsync(ImportRequest request);
}

[RemoteService]
public interface IExportInvoicesUseCase
{
    [RemoteMethod]
    Task<FileContent> ExportAsync(int year);     // a streamed download: the caller disposes it
}
```

On the server an uploaded file is a **seekable temp file of known length, deleted when the call ends**: copy it
during the call if you keep it. Its name and content type are sanitized when it arrives remotely, but **not**
when the use case runs locally: never use `FileName` as a path without your own checks.

On the client, **dispose a downloaded `FileContent`**: it holds the HTTP connection until disposed.

```csharp
using (var file = await export.ExportAsync(2026))
using (var target = File.Create(Path.Combine(folder, file.FileName)))
    await file.Content.CopyToAsync(target);
```

### 3.4 The convention test

Nothing validates the app's contracts in depth at startup (descriptors are built lazily, on the first call). Make
it a unit test, over every assembly that declares or implements use cases:

```csharp
using A5Soft.CARMA.Application.Conventions;

public class DataPortalConventionTests
{
    [Theory]
    [InlineData(typeof(Acme.Core.IGreetUseCase))]          // one type per assembly to check
    public void UseCasesFollowTheDataPortalConventions(Type typeInAssembly)
        => DataPortalConventions.EnsureValid(typeInAssembly.Assembly);   // throws, listing every violation
}
```

`DataPortalConventions.Validate(assembly)` returns the violations as a list (`ConventionViolation`: type, member
path, message) if you prefer your own assertion. It walks every use case and lookup type with the data portal's own
contract resolver and also reports singleton services taking scoped or transient services. With 10–20k use cases
expect tens of seconds.

---

## 4. App-level use case bases

**The data portal is called only from app-level base classes, never from a concrete use case.** A concrete use case
implements the local work; the base decides whether to run it here or send the call. The same class runs in every
build:

| Build | `IsRemote` | What runs |
|---|---|---|
| Server (call arriving through the portal) | false (local endpoint) | the local work |
| ASP.NET with UI, thick client working locally | false | the local work |
| Thin client, thick client connected to a server | true | the portal call |

```csharp
using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;

public abstract class QueryUseCaseBase<TCriteria, TResult>
{
    private readonly IAuthenticationStateProvider _authentication;

    protected QueryUseCaseBase(IClientDataPortal dataPortal, IAuthenticationStateProvider authentication)
    {
        DataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
        _authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
    }

    protected IClientDataPortal DataPortal { get; }

    /// <summary>The remote method: the same signature as on the interface.</summary>
    public async Task<TResult> ExecuteAsync(TCriteria criteria)
    {
        if (DataPortal.IsRemote)
        {
            // GetType(): the implementation; the portal finds its remote interface
            var identity = await _authentication.GetIdentityAsync();
            var (result, refreshed) = await DataPortal.FetchAsync<TCriteria, TResult>(GetType(), criteria,
                identity?.IsAuthenticated == true ? identity : null);
            if (null != refreshed) await _authentication.NotifyIdentityChangedAsync(refreshed);
            return result;
        }

        // local: authorize here (the server authorizes remote calls), then do the work
        return await DoExecuteAsync(criteria);
    }

    protected abstract Task<TResult> DoExecuteAsync(TCriteria criteria);
}

[DefaultServiceImplementation(typeof(IGreetUseCase))]          // Any: the class decides where it runs
public sealed class GreetUseCase : QueryUseCaseBase<string, Greeting>, IGreetUseCase
{
    private readonly IGreetingRepository _repository;

    public GreetUseCase(IClientDataPortal dataPortal, IAuthenticationStateProvider authentication,
        IGreetingRepository repository) : base(dataPortal, authentication)
        => _repository = repository;

    protected override async Task<Greeting> DoExecuteAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new AcmeBusinessException("A name is required.");
        return new Greeting { Text = await _repository.GetGreetingAsync(name), CreatedAt = DateTime.UtcNow };
    }
}
```

Rules for the bases:

- **`GetType()`** as the use case type: the implementation; the portal maps it to its single remote interface.
- **Pick the overload by shape**: `FetchAsync<TResult>`, `FetchAsync<TArg, TResult>`, ... up to three arguments;
  `InvokeAsync(...)` for a method whose result you discard. A save is a `FetchAsync<TEntity, TEntity>`.
- **Identity**: pass the identity holding the token claim; `null` for an anonymous call (only for use cases the
  server allows anonymously). An unauthenticated `ClaimsIdentity` without the claim throws, so map it to `null`.
- **Refreshed identity**: when the second tuple item is not `null`, replace the session's identity with it, or a
  permission granted mid-session never takes effect.
- **Authorization**: on the local branch only (the server authorizes remote calls with its own rules, see §9); the
  remote branch must not trust the client.
- **Domain objects** in results are rebuilt with the receiving scope's `Domain.IApplicationContext` (validation
  engine, metadata, binding mode).
- **Logging**: log entry/exit and errors in the base, once.
- **No cancellation token** reaches the portal; long calls are bounded by the transport's `CallTimeout`.

---

## 5. Lookups

The framework's lookup services (`LookupServiceBase<TLookup>`, `LookupListServiceBase<TLookup>`,
`GenericLookupService`) follow the same pattern: remote → `FetchAsync<Type, TLookup>(GetType(), requesterType,
identity)`; local → authorize the requester and fetch. Requirements:

- The **requester** (the use case asking for the lookup) is sent as a `Type` value: it must be a use case
  implementation (a trusted wire type) whose `[UseCase(..., requiredLookups)]` lists the lookup type.
- The generic lookup service sends its lookup and entity types as generic arguments; both must be trusted wire
  types (a lookup type deriving from `LookupBase<T>` and its `T` are, automatically).
- Lookups need a private `[SerializationConstructor]`.
- `IClientDataPortal` is registered in every build for this reason (lookups and bases branch on `IsRemote`
  everywhere).

---

## 6. The version provider

```csharp
using A5Soft.CARMA.Application;
using A5Soft.CARMA.Domain;

// in an assembly every build ships (e.g. Acme.Core), so that client and server report the same version
[DefaultServiceImplementation(typeof(IAppVersionProvider))]
public sealed class AcmeVersionProvider : IAppVersionProvider
{
    public string AppName => "Acme";

    // the numeric part only: an informational suffix (1.4.0-beta) is not a Version
    public Version AppVersion { get; } = typeof(AcmeVersionProvider).Assembly.GetName().Version;
}
```

- Registered on clients and server; there is deliberately no default and no configuration setting.
- Read the version of an assembly that **both** client and server ship in the same version (a shared contracts
  assembly), or a constant, so that client and server report the same version for the same release.
- `AppName` must be equal on both sides (ordinal comparison).
- The server refuses to start with a missing name or version; a client fails its first call.
- How strictly versions are compared is the **client's** `DataPortalClientOptions.AppVersionMatch` (`Exact`,
  `MajorMinor` default, `None`).

---

## 7. The remote error mapper

```csharp
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;

[DefaultServiceImplementation(typeof(IRemoteErrorMapper))]
public sealed class AcmeRemoteErrorMapper : IRemoteErrorMapper
{
    // server: exception → error; null = ServerError (correlation id only)
    public RemoteError ToError(Exception exception)
    {
        switch (exception)
        {
            case BusinessException business:
                return new RemoteError { Kind = RemoteErrorKind.Business, Message = business.Message };
            case UnauthorizedAccessException _:
                return new RemoteError { Kind = RemoteErrorKind.Unauthorized };
            default:
                return null;
        }
    }

    // client: error → exception; null = RemoteCallException
    public Exception ToException(RemoteError error)
    {
        switch (error.Kind)
        {
            case RemoteErrorKind.Business: return new AcmeBusinessException(error.Message);
            case RemoteErrorKind.Unauthorized: return new UnauthorizedAccessException(error.Message);
            default: return null;
        }
    }
}
```

Guidance per kind:

| Kind | Map from (server) | Message | Data |
|---|---|---|---|
| `Business` | your business exceptions | localized message for the user (the caller's culture is applied before the call) | optional |
| `Validation` | your validation exception | summary | broken rules (e.g. `JToken.FromObject(brokenRules)`) |
| `Concurrency` | your concurrency exception | message | optional |
| `Unauthorized` | not-authenticated / not-authorized exceptions | optional | — |
| `ServiceUnavailable` | a transient exception that survived the server's retries | optional | — |
| anything else | return `null` | **never copy the message of a non-business exception** (it may leak internals) | — |

- `ServerError` and `BadRequest` are always stripped of message and data on the server.
- The mapper must not throw; if `ToError` throws, the failure is logged and reported as `ServerError`.
- On the client, `ToException` also receives errors the client created itself (transport failures, version
  mismatches): handle `VersionMismatch` and `ServiceUnavailable` there if the UI needs specific exception types.
- Exception `Data` must be JSON-serializable with the portal's settings; if not, the endpoint falls back to an error
  without data.

Reading a version mismatch:

```csharp
catch (RemoteCallException ex) when (ex.Kind == RemoteErrorKind.VersionMismatch)
{
    var details = ex.Error.Data?.ToObject<VersionMismatchData>();
    // details.Reason, ClientAppVersion, ServerAppVersion, ClientPluginFingerprint, ServerPluginFingerprint
}
```

---

## 8. The authentication state provider and the token

### 8.1 The token claim

The client data portal authenticates a call with **one claim** of the caller's identity: its type is
`DataPortalClientOptions.TokenClaimType` (default `"carma:token"`), its value the token the server issued.

- The token must be **printable ASCII without spaces** (it travels as `Authorization: Carma <token>`). JWTs are.
- The server verifies it in `VerifyIdentityAsync` (§9) and derives the identity from it; the other claims the
  client holds are not trusted.
- How the token is obtained is the app's choice: an anonymous login use case returning a token, an identity
  provider (OIDC), Windows authentication on the server issuing a token, ...

### 8.2 `IAuthenticationStateProvider`

The current identity of a scope. One implementation can serve both sides:

```csharp
using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Domain;

[DefaultServiceImplementation(typeof(IAuthenticationStateProvider))]
public sealed class ScopeAuthenticationStateProvider : IAuthenticationStateProvider
{
    private Task<ClaimsIdentity> _identity = Task.FromResult(new ClaimsIdentity());

    public event IdentityChangedHandler IdentityChanged;

    public Task<ClaimsIdentity> GetIdentityAsync() => _identity;

    public Task NotifyIdentityChangedAsync(ClaimsIdentity updatedIdentity)
    {
        _identity = Task.FromResult(updatedIdentity ?? throw new ArgumentNullException(nameof(updatedIdentity)));
        IdentityChanged?.Invoke(_identity);
        return Task.CompletedTask;
    }
}
```

- Client: the session sets it at login and on every refreshed identity.
- Server: the server portal sets it in `VerifyIdentityAsync` for the call's scope.
- Clients that revalidate the identity periodically can derive from `AuthenticationStateProviderBase` instead.

### 8.3 The logger

`A5Soft.CARMA.Application.ILogger` has no default; adapt your logging framework (a complete adapter over
`Microsoft.Extensions.Logging` is `AcmeLogger` in `samples/Acme.Host/AppServices.cs`). The server data portal logs refused
calls and failures with their correlation ids through it.

---

## 9. The server data portal

```csharp
using A5Soft.CARMA.Application;
using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Application.DataPortal.Protocol;
using A5Soft.CARMA.Application.DataPortal.Server;
using A5Soft.CARMA.Application.IoC.Plugins;
using Microsoft.Extensions.DependencyInjection;

public sealed class AcmeServerDataPortal : ServerDataPortalBase
{
    public const string TenantClaimType = "acme:tenant";
    public const string PluginClaimType = "acme:plugin";

    private readonly ITokenService _tokens;
    private readonly IAuthorizationProvider _authorization;

    public AcmeServerDataPortal(RemoteUseCaseRegistry registry, TenantServiceMap tenantServices,
        PluginCatalog plugins, IRemoteErrorMapper errorMapper, IDataPortalServerOptions options,
        IAppVersionProvider versionProvider, ILogger logger, ITokenService tokens,
        IAuthorizationProvider authorization)
        : base(registry, tenantServices, plugins, errorMapper, options, versionProvider, logger)
    {
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
    }

    protected override async Task<ClaimsIdentity> VerifyIdentityAsync(string token, IServiceProvider services)
    {
        var identity = _tokens.Verify(token);   // null: invalid or expired, the call is refused
        if (null == identity) return null;

        // the call's scope: the use cases read the identity from here
        await services.GetRequiredService<IAuthenticationStateProvider>().NotifyIdentityChangedAsync(identity);
        return identity;
    }

    protected override Task AuthorizeAsync(object useCase, RemoteUseCaseDescriptor descriptor,
        ClaimsIdentity identity, IServiceProvider services)
    {
        if (useCase is IAuthorizedUseCase)
        {
            if (null == identity) throw new UnauthorizedAccessException("Log in first.");
            _authorization.GetAuthorizer(useCase.GetType()).IsAuthorized(identity, true);
        }
        else if (null == identity && !descriptor.InterfaceType.IsDefined(typeof(AllowAnonymousAttribute), false))
        {
            // anonymous calls only for the use cases marked as such (login, registration...)
            throw new UnauthorizedAccessException("Log in first.");
        }

        return Task.CompletedTask;
    }

    protected override string GetTenantId(ClaimsIdentity identity) => identity?.FindFirst(TenantClaimType)?.Value;

    protected override IReadOnlyCollection<string> GetTenantPlugins(ClaimsIdentity identity)
        => identity.FindAll(PluginClaimType).Select(c => c.Value).ToArray();
}
```

### 9.1 Overrides

| Member | Required | Contract |
|---|---|---|
| `VerifyIdentityAsync(token, services)` | yes | Return the verified identity, or `null` (→ `Unauthorized`). Make it the current identity of `services` (the call's scope). Check signature **and** expiry. Runs before `Expect: 100-continue` is answered: keep it fast. |
| `AuthorizeAsync(useCase, descriptor, identity, services)` | yes | Throw if the caller may not invoke the use case. `identity == null` is an anonymous call: refuse unless the use case is meant for it. Runs before any data is read: decide from the use case and the identity only. The exception goes through the mapper. |
| `GetRefreshedIdentityAsync(identity, services)` | no (`null`) | Return a new identity (with a new token claim) to send back, e.g. when the token is about to expire or permissions changed. |
| `GetTenantId(identity)` | tenant-based apps | The tenant from a claim the server issued. Default: no tenant. |
| `GetTenantPlugins(identity)` | tenant-based apps | The plugin ids from claims the server issued after `ValidateAssignment`. Called only with a tenant. |

The constructor also refuses a version provider without name or version, so a misconfigured server fails at
startup.

### 9.2 Registration and lifetime

`ServerDataPortalBase` has no interface; register the derived class under its own type, for `Server` only. With
the scan:

```csharp
public sealed class HostDependencies : IAssemblyDependencies
{
    public IReadOnlyCollection<ExternalServiceDescriptor> ExternalServices { get; } = new[]
    {
        new ExternalServiceDescriptor(typeof(AcmeServerDataPortal), null,
            ServiceLifetime.Singleton, BuildConfiguration.Server)
    };

    public IReadOnlyCollection<UnmanagedAssemblyDescriptor> UnmanagedAssemblies { get; }
        = Array.Empty<UnmanagedAssemblyDescriptor>();
}
```

or in `Startup`: `services.AddSingleton<AcmeServerDataPortal>()`.

- **Singleton** when every constructor dependency is a singleton (the framework's are); per-call state lives in
  `services` (the request scope). Resolve scoped services from `services`, never take them in the constructor of a
  singleton.
- **Scoped** if it must take scoped services in its constructor.

### 9.3 The server's use cases see

- the identity: through `IAuthenticationStateProvider` of the request scope (set in `VerifyIdentityAsync`);
- the caller's culture: `CultureInfo.CurrentCulture`/`CurrentUICulture` are applied for the call;
- the tenant: `ITenantContext` of the request scope;
- uploaded files: seekable, deleted after the call.

---

## 10. Client sessions: connection, identity, tenant

A client creates **one DI scope per session**: a server (endpoint), a user (identity) and a tenant. Changing any of
them disposes the session and opens a new one; nothing in a scope changes its endpoint or tenant.

```csharp
using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Application.DataPortal.Client;
using A5Soft.CARMA.Application.Tenancy;
using Microsoft.Extensions.DependencyInjection;

public sealed class ClientSession : IDisposable
{
    private readonly IServiceScope _scope;

    private ClientSession(IServiceScope scope) => _scope = scope;

    public IServiceProvider Services => _scope.ServiceProvider;

    /// <param name="endpoint">the server and transport chosen at login; null for the configured one</param>
    /// <param name="identity">the identity received at login (holding the token claim); null before login</param>
    public static async Task<ClientSession> OpenAsync(IServiceProvider root, DataPortalEndpoint endpoint,
        ClaimsIdentity identity)
    {
        var scope = root.CreateScope();
        try
        {
            var services = scope.ServiceProvider;
            // the endpoint first: nothing may read it before
            if (null != endpoint) services.GetRequiredService<IDataPortalConnection>().Initialize(endpoint);

            // check the server before anything else is sent: another app, version or plugins fail here
            var connection = services.GetRequiredService<IDataPortalConnection>();
            if (connection.IsRemote)
                await services.GetRequiredService<IServerSettingsProvider>().GetAsync(connection.Endpoint);

            if (null != identity)
                await services.GetRequiredService<IAuthenticationStateProvider>().NotifyIdentityChangedAsync(identity);

            // the tenant and its plugins, as the server put them into the identity
            var tenantId = identity?.FindFirst(AcmeServerDataPortal.TenantClaimType)?.Value;
            var plugins = identity?.FindAll(AcmeServerDataPortal.PluginClaimType).Select(c => c.Value);
            services.GetRequiredService<ITenantContext>().Initialize(tenantId, plugins);

            return new ClientSession(scope);
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }

    public void Dispose() => _scope.Dispose();
}
```

Usage:

```csharp
// the login screen: user chooses a server (or "work locally" in a thick client)
var endpoint = workLocally
    ? new DataPortalEndpoint(DataPortalEndpoint.Local)
    : new DataPortalEndpoint(DataPortalEndpoint.Http, new Uri("https://acme.example/api/dataportal"));

using (var session = await ClientSession.OpenAsync(root, endpoint, identity))
{
    var greeting = await session.Services.GetRequiredService<IGreetUseCase>().ExecuteAsync("Ann");
}
```

- Order matters: **connection first** (reading it fixes it), then identity, then tenant (before resolving anything
  tenant scoped).
- `IServerSettingsProvider.GetAsync` at session start turns an incompatible server into an immediate, clear error at
  login rather than on the first use case.
- Blazor Server: the circuit's scope is the session; with a configured app server no `Initialize` is needed.
  Blazor WebAssembly: the app's root scope is one user; create a scope per login if the user can switch server or
  tenant.
- The thick client's "work locally" uses the local endpoint: the use cases run in process on local repositories,
  and the **local branch must authorize** (§4).

---

## 11. Handling data portal errors in the UI

| Exception | Kind | UI reaction |
|---|---|---|
| app exception from the mapper (e.g. `AcmeBusinessException`) | `Business`, `Validation`, `Concurrency` | show the message / broken rules; offer reload on concurrency |
| `UnauthorizedAccessException` (mapped) or `RemoteCallException` `Unauthorized` | `Unauthorized` | log in again or enter the tenant again |
| `RemoteCallException` `ServiceUnavailable` | server unreachable, timeout, gateway errors | "try again later"; retry is safe for queries |
| `RemoteCallException` `VersionMismatch` | another app, version, protocol or plugins | ask the user to update/reload the client |
| `RemoteCallException` `BadRequest` | limits exceeded before sending, malformed call or answer | show a generic error; the inner exception says which limit (client side) |
| `RemoteCallException` `ServerError` | unexpected server failure | show the correlation id for support |
| `IOException` while reading a downloaded file | download aborted mid-stream | retry the download |

---

## 12. A custom transport

Implement `IDataPortalTransport` (a unique `Name`, `RequiresAddress`, `CreateProxy`) and an `IDataPortalProxy`:

```csharp
[DefaultServiceImplementation(typeof(IDataPortalTransport), BuildConfiguration.Client)]
public sealed class GrpcDataPortalTransport : IDataPortalTransport
{
    public string Name => "grpc";
    public bool RequiresAddress => true;
    public IDataPortalProxy CreateProxy(DataPortalEndpoint endpoint) => new GrpcDataPortalProxy(endpoint.Address);
}
```

A proxy must:

- deliver `DataPortalRequest.Call` before the data (so the server can refuse without receiving files), then the
  payload, then the files in order;
- read each file once, never dispose it, and resend a call only while `request.FilesRead` is false;
- return a response as soon as it is known, with a downloaded file still arriving; disposing the file must release
  the transport;
- throw `DataPortalTransportException` (kind `ServiceUnavailable`, `ServerError`, `BadRequest`, `VersionMismatch`)
  when no data portal response was received; let programming errors propagate;
- fetch the server settings without authentication;
- hold no per-user state (one proxy per endpoint, shared by everyone).

The server side of a custom transport translates its messages into `DataPortalCall` + an `IDataPortalRequestBody`,
calls `ServerDataPortalBase.HandleAsync`, and writes the `DataPortalResponse` back; the HTTP endpoint
([04](04-aspnetcore-endpoint.md)) is the reference implementation.

---

## 13. Pitfalls

| Pitfall | Effect | Avoid by |
|---|---|---|
| Calling `IClientDataPortal` from a concrete use case | every use case changes when the call shape changes | only in app-level bases |
| A class implementing two remote interfaces | `GetType()` cannot be mapped to one interface: the call fails | one remote interface per class |
| Passing an unauthenticated `ClaimsIdentity` | `InvalidOperationException` (no token claim) | pass `null` for anonymous |
| Ignoring the refreshed identity | stale permissions, expired token | always apply it |
| Authorizing only on the server | a thick client working locally runs unauthorized | authorize on the local branch |
| Public or missing serialization constructor on a lookup/query result | fails on first call | private `[SerializationConstructor]`, names and types matching |
| `int` constructor parameter for a `long` property | refused on first use | same types |
| Domain object inside a list or POCO | refused by the conventions | pass DTOs inside, or make it the top-level value |
| Keeping an uploaded `FileContent` after the call | file deleted | copy during the call |
| Not disposing a downloaded `FileContent` | HTTP connections exhausted | `using` |
| Trusting `FileContent.FileName` | path traversal when running locally | sanitize/choose the path yourself |
| Renaming/moving a use case interface | old clients: unknown use case | treat as a protocol change; bump the version |
| `AppVersion` read from an assembly versioned differently on client and server | spurious mismatches | read a shared assembly's version, or a constant |
| Non-ASCII or space in the token | the HTTP proxy refuses to send | ASCII tokens (JWT) |
| A server portal singleton taking a scoped dependency | captured scoped service | resolve from `services` |
| Forgetting `AuthorizeAsync` for anonymous calls | anonymous access to everything | refuse `identity == null` except for marked use cases |
| Mapper copying `exception.Message` for every exception | internals leak to clients | map only business exceptions; return `null` otherwise |
