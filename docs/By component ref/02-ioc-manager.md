# 2. Implementing the IoC manager and related components

The IoC manager turns the app's assemblies into container registrations. The framework does the discovery,
composition and validation; the app supplies only what depends on its host: which assemblies, which container,
how configuration is bound, how plugins and native libraries are loaded.

**Contents**

1. [What the framework does and what the app does](#1-what-the-framework-does-and-what-the-app-does)
2. [Declaring services](#2-declaring-services)
3. [Choosing the build configuration of an implementation](#3-choosing-the-build-configuration-of-an-implementation)
4. [Composition rules](#4-composition-rules)
5. [Implementing the IoC manager](#5-implementing-the-ioc-manager)
6. [What `Initialize` does, in order](#6-what-initialize-does-in-order)
7. [Hosting the IoC manager in each deployment](#7-hosting-the-ioc-manager-in-each-deployment)
8. [Configuration classes](#8-configuration-classes)
9. [Plugins](#9-plugins)
10. [Tenancy](#10-tenancy)
11. [The HTTP connection hook](#11-the-http-connection-hook)
12. [Services every app must provide](#12-services-every-app-must-provide)
13. [Testing the composition](#13-testing-the-composition)
14. [Pitfalls](#14-pitfalls)

---

## 1. What the framework does and what the app does

| Framework (`IoCManagerBase.Initialize`) | App (derived class) |
|---|---|
| Scans the framework's and the app's assemblies once | Lists the app's assemblies (`AppAssemblies`) |
| Finds services, configurations, remote services, trusted wire types, external services, native libraries | Registers one service in its container (`AddService`), and a factory (`AddServiceFactory`) if tenant based |
| Composes: app replaces framework defaults, duplicates refused | Binds a configuration from its configuration source (`BindConfiguration`) |
| Loads, validates and composes plugins | Loads a plugin assembly (`LoadPlugin`) and a native library (`LoadUnmanagedAssembly`) |
| Creates, binds, initializes and validates configurations | Optionally: extra system services, an HTTP handler, plugin hooks, native path resolution |
| Builds the registry, plugin catalog, tenant service map, build info, domain settings | Builds the container and creates scopes |

Loading assemblies and native libraries is left to the app because neither `AssemblyLoadContext` nor `NativeLibrary`
is part of .NET Standard 2.1.

---

## 2. Declaring services

### 2.1 Interface + default implementation

```csharp
using A5Soft.CARMA.Domain;

[Service(ServiceLifetime.Scoped)]                        // lifetime, allowMultipleImplementations = false
public interface IGreetingRepository
{
    Task<string> GetGreetingAsync(string name);
}

[DefaultServiceImplementation(typeof(IGreetingRepository), BuildConfiguration.Backend)]
public sealed class GreetingRepository : IGreetingRepository { ... }
```

- The **interface** carries `[Service]` (or `[RemoteService]` / `[UseCase]`, which derive from it): the lifetime
  (`Transient` by default) and whether several implementations may coexist. `[Service]` is inherited by derived
  interfaces.
- The **class** carries `[DefaultServiceImplementation(interfaceType, forBuildConfiguration = Any)]`. Only concrete,
  non-generic classes are scanned; the attribute is not inherited.
- An implementation whose interface has no `[Service]` stops startup:
  *"... declares that it implements interface ... however this interface does not have a ServiceAttribute."*
- One class implements one service interface through the attribute (`AllowMultiple = false` on the attribute).

### 2.2 Services that allow multiple implementations

`[Service(ServiceLifetime.Singleton, true)]` (e.g. `IDataPortalTransport`, `IHealthCheck`): every implementation in
the build is registered, and consumers take `IEnumerable<T>`. The framework's implementations are kept beside the
app's and the plugins'.

### 2.3 External services: `IAssemblyDependencies`

Types the scan cannot mark (a third-party class, or a class with no interface) are declared by one class per
assembly implementing `IAssemblyDependencies`:

```csharp
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.IoC;

public sealed class HostDependencies : IAssemblyDependencies
{
    public IReadOnlyCollection<ExternalServiceDescriptor> ExternalServices { get; } = new[]
    {
        // implementation, interface (null: registered under its own type), lifetime, build, allowMultiple
        new ExternalServiceDescriptor(typeof(AcmeServerDataPortal), null,
            ServiceLifetime.Singleton, BuildConfiguration.Server)
    };

    public IReadOnlyCollection<UnmanagedAssemblyDescriptor> UnmanagedAssemblies { get; } = new[]
    {
        // no extension: .dll/.so/.dylib is added per platform; relative to the app path (or plugin folder)
        new UnmanagedAssemblyDescriptor("native/libsodium", BuildConfiguration.Any)
    };
}
```

- Only one `IAssemblyDependencies` class per assembly (two stop startup). It may have a private constructor.
- External services are never tenant scoped and never remote.

### 2.4 Health checks

`[WithHealthCheck(typeof(MyHealthCheck))]` on a service implementation registers `MyHealthCheck` as an
`IHealthCheck` singleton (multiple allowed) for the same build configuration as the implementation.
Use one `[WithHealthCheck]` per class: the attribute allows several, but the scan reads a single one and fails
with `AmbiguousMatchException` on two.

---

## 3. Choosing the build configuration of an implementation

Ask *where is this implementation needed*, and pick the narrowest value that covers it:

| Value | Use for | Registered in |
|---|---|---|
| `Any` (default) | Works the same everywhere, using no server resources: formatting, calculations, metadata. **Use case implementations** (the class decides at run time whether to call the data portal or run locally, so it must exist in every build). | all four builds |
| `Backend` | The backend logic wherever it runs: **repositories**, domain services touching the database. | `Server`, `Client \| Server`, `Client \| Local` |
| `Server` | Only a server host: the server data portal class, background jobs, token issuing, server health checks. Not repositories (a thick client working locally needs them). | `Server`, `Client \| Server` |
| `Local` | Only a thick client working locally: creating/migrating the local database, local backup. | `Client \| Local` |
| `Client` | The UI side: shell services, view models, session state. | `Client`, `Client \| Server`, `Client \| Local` |

**One implementation per build.** An interface may have several implementations for different roles, as long as no
build registers two of them: `Client` meets `Server` (ASP.NET with UI), `Client` meets `Local` and `Backend` (thick
client), `Any` meets everything; only `Server` and `Local` never meet.

**Thin clients and backend dependencies.** A use case implementation (registered everywhere) takes its repositories
in its constructor. A thin client has no repositories, so its container cannot build the use case. Put repository
**stubs** in a separate assembly that only the thin client lists in `AppAssemblies`:

```csharp
// Acme.Stubs: only in the thin client's AppAssemblies
[DefaultServiceImplementation(typeof(IGreetingRepository))]
public sealed class GreetingRepositoryStub : IGreetingRepository
{
    public Task<string> GetGreetingAsync(string name)
        => throw new NotSupportedException("A thin client has no backend.");
}
```

The stubs are never called: in a thin client `IsRemote` is always true, so the use case base always goes to the
data portal.

> The XML comment on `DefaultServiceImplementationAttribute` currently recommends `Backend` for use case
> implementations. With the app-level use case bases that branch on `IsRemote` (the pattern in
> [03 §4](03-app-data-portal-components.md#4-app-level-use-case-bases)), use cases must be `Any`; only their
> backend dependencies are `Backend`. Update that comment accordingly.

---

## 4. Composition rules

1. **App and plugins over framework.** For a single-implementation interface, a framework default is registered
   only if no app assembly implements the interface. Framework services with `AllowMultiple` are always kept.
2. **No duplicates in the app.** Two app implementations of one single-implementation interface in the same build
   stop startup (*"has more than one default implementation"*). A service registered without an interface
   (external, `null` interface) is a duplicate only of the same implementation type.
3. **Plugins override the app** (see §9): one plugin per app scoped service or singleton; tenant scoped services
   per plugin.
4. **System services** (`AddSystemServices`) are added last and can never be overridden: an interface that anything
   else implements stops startup, unless it allows multiple implementations.
5. **Tenant scoped** services: not singletons; an app scoped service may not depend on one (§10).

---

## 5. Implementing the IoC manager

### 5.1 Constructor

```csharp
protected IoCManagerBase(BuildConfiguration buildConfiguration, bool isDevelopment, BindingMode bindingMode,
    string appPath = null)
```

| Parameter | Meaning | Pitfalls |
|---|---|---|
| `buildConfiguration` | `Server`, `Client`, `Client \| Server` or `Client \| Local` | Anything else (`Any`, `Backend`, `Local`, `0`) throws `ArgumentException`. |
| `isDevelopment` | Passed to every `ConfigurationBase.Init` | |
| `bindingMode` | The UI framework's binding mode for every domain object (`WinForms`, `Wpf`); exposed as `IDomainSettings` | Not configuration: fixed by the kind of app. Irrelevant on a server. |
| `appPath` | Base path for relative paths (native libraries, plugins folder, spool directory); default `AppContext.BaseDirectory` | Must exist. In ASP.NET use `ContentRootPath`. |

### 5.2 Members

| Member | Kind | Implement |
|---|---|---|
| `AppAssemblies` | abstract | The app's assemblies for this deployment. The framework's are added automatically; at least one app assembly is required. Duplicates and framework assemblies are ignored. |
| `AddService(ApplicationServiceInfo)` | abstract | Register `ImplementationInstance` as a singleton if set; otherwise `ImplementationType` with `Lifetime`, under `InterfaceType` (or the implementation type if `null`). **Add, never TryAdd.** |
| `BindConfiguration(ConfigurationBase)` | abstract | Fill the instance from the configuration source, e.g. `configuration.GetSection(instance.RootSection).Bind(instance)`. A missing section must be a no-op. |
| `LoadUnmanagedAssembly(string fullPath)` | abstract | E.g. `NativeLibrary.Load(fullPath)`. Called only if an assembly declares one. |
| `AddServiceFactory(Type, ServiceLifetime, Func<IServiceProvider, object>)` | virtual (throws) | Register a factory; needed only if something is `[TenantScoped]`. |
| `LoadPlugin(string libraryPath)` | virtual (throws) | Load a plugin assembly (own `AssemblyLoadContext`); needed only if the plugins folder has plugins. |
| `OnPluginAccepted(PluginAssembly)` | virtual | E.g. scan the plugin for the app's own plugin interfaces. Throwing stops startup. |
| `AddSystemServices(plugins, SystemServiceCollection)` | virtual | Add services computed from the plugins; they cannot be overridden. |
| `CreateHttpMessageHandler()` | virtual (`null`) | The HTTP data portal connection settings (§11). |
| `ResolveUnmanagedPath(basePath, descriptor)` | virtual | Default: `basePath/descriptor.Path` + `.dll`/`.dylib`/`.so`. Override for per-architecture folders. |
| `Services`, `TenantServices`, `Plugins` | properties | What was registered and accepted, after `Initialize` (diagnostics, tests). |

### 5.3 A complete implementation (Microsoft.Extensions.DependencyInjection)

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using A5Soft.CARMA.Application;
using A5Soft.CARMA.Application.IoC;
using A5Soft.CARMA.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// CARMA has its own ServiceLifetime: alias Microsoft's to avoid an ambiguous reference
using MsServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;

namespace Acme.Host
{
    public sealed class AcmeIoCManager : IoCManagerBase
    {
        private readonly IServiceCollection _services;
        private readonly IConfiguration _configuration;
        private readonly Assembly[] _appAssemblies;
        private readonly Func<HttpMessageHandler> _httpHandlerFactory;

        public AcmeIoCManager(IServiceCollection services, IConfiguration configuration, BuildConfiguration build,
            bool isDevelopment, BindingMode bindingMode, IEnumerable<Assembly> appAssemblies,
            Func<HttpMessageHandler> httpHandlerFactory = null, string appPath = null)
            : base(build, isDevelopment, bindingMode, appPath)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _appAssemblies = new List<Assembly>(appAssemblies ?? throw new ArgumentNullException(nameof(appAssemblies)))
                .ToArray();
            _httpHandlerFactory = httpHandlerFactory;
        }

        protected override IEnumerable<Assembly> AppAssemblies => _appAssemblies;

        protected override void AddService(ApplicationServiceInfo service)
        {
            var serviceType = service.InterfaceType ?? service.ImplementationType;

            // Add, never TryAdd: an interface that allows multiple implementations (e.g. the data portal
            // transports) is registered once per implementation and resolved as IEnumerable<T>
            if (null != service.ImplementationInstance)
                _services.Add(new ServiceDescriptor(serviceType, service.ImplementationInstance));
            else
                _services.Add(new ServiceDescriptor(serviceType, service.ImplementationType, Map(service.Lifetime)));
        }

        protected override void AddServiceFactory(Type serviceType, A5Soft.CARMA.Domain.ServiceLifetime lifetime,
            Func<IServiceProvider, object> factory)
            => _services.Add(new ServiceDescriptor(serviceType, factory, Map(lifetime)));

        // a missing section binds nothing: every setting keeps its default
        protected override void BindConfiguration(ConfigurationBase configuration)
            => _configuration.GetSection(configuration.RootSection).Bind(configuration);

        protected override void LoadUnmanagedAssembly(string fullPath) => NativeLibrary.Load(fullPath);

        protected override Assembly LoadPlugin(string libraryPath)
            => new PluginLoadContext(libraryPath).LoadFromAssemblyPath(libraryPath);

        protected override HttpMessageHandler CreateHttpMessageHandler() => _httpHandlerFactory?.Invoke();

        private static MsServiceLifetime Map(A5Soft.CARMA.Domain.ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case A5Soft.CARMA.Domain.ServiceLifetime.Singleton: return MsServiceLifetime.Singleton;
                case A5Soft.CARMA.Domain.ServiceLifetime.Scoped: return MsServiceLifetime.Scoped;
                default: return MsServiceLifetime.Transient;
            }
        }
    }
}
```

One class serves every deployment; each host passes its build configuration and assemblies.

### 5.4 Other containers

Any container works if it can:

- register an **instance** as a singleton, a **type** with transient/scoped/singleton lifetime, and a **factory**
  receiving the current scope's `IServiceProvider` (for tenant scoped services);
- register **several implementations of one interface** and inject them as `IEnumerable<T>`;
- create **internal** classes through their public constructors (several framework defaults are internal);
- create a **scope** per request/session.

Autofac: `builder.RegisterInstance(...)`, `RegisterType(...).As(...).InstancePerLifetimeScope()`,
`Register(ctx => factory(ctx.Resolve<IServiceProvider>()))`. With `Autofac.Extensions.DependencyInjection`, keep
the IoC manager writing to the `IServiceCollection` and let `Populate` do the rest.

---

## 6. What `Initialize` does, in order

Call `Initialize()` once, at startup, before the container is built. Everything that can be wrong fails here.

| # | Step | Fails when |
|---|---|---|
| 1 | Checks: not initialized yet; at least one app assembly | called twice; no app assembly |
| 2 | `CreateHttpMessageHandler()`; a non-null handler is installed for the HTTP transport | handler already set in this process, or in a browser |
| 3 | Scans the framework and app assemblies; merges remote services and trusted wire types by wire name | two types share a wire name; an assembly's types cannot be loaded (the loader errors are listed) |
| 4 | Loads the app's native libraries (for this build) | a declared file is missing |
| 5 | Composes the app's services over the framework's (§4) | two app implementations of one single service |
| 6 | Creates `PluginsConfiguration` first, then loads every plugin folder, checks ids, validates and adds each plugin in id order (§9) | a folder without the library; no `PluginId`; duplicate ids; any plugin violation |
| 7 | Builds the composition: app services, plugin overrides, tenant scoped services | — |
| 8 | `AddSystemServices(...)` | a system service's interface already implemented; tenant scoped; lifetime differs from `[Service]` |
| 9 | Checks that no app scoped service depends on a tenant scoped one (every public constructor; `IEnumerable<T>` included) | such a dependency |
| 10 | Creates every configuration for this build: `new` → `BindConfiguration` → `Init(appPath, isDevelopment)` → `Validate()` | a configuration cannot be created, or `Validate` throws |
| 11 | Adds `RemoteUseCaseRegistry`, `PluginCatalog`, `TenantServiceMap`, `IDomainSettings`, `IBuildInfo` as singleton instances | — |
| 12 | `AddService` for every service; for each tenant scoped interface: each implementation under its own type, the interface through `AddServiceFactory` | — |

Not done at startup: the in-depth data portal convention checks of the **app's** assemblies (they belong in a
unit test, [03 §3.4](03-app-data-portal-components.md#34-the-convention-test)); the singleton→singleton dependency
check (also in the unit test). Plugins *are* checked in depth at startup, because no app test covers them.

---

## 7. Hosting the IoC manager in each deployment

### 7.1 Server (`Server`)

```csharp
public void ConfigureServices(IServiceCollection services)
{
    new AcmeIoCManager(services, _configuration, BuildConfiguration.Server, _environment.IsDevelopment(),
        BindingMode.WinForms,   // irrelevant on a server: no UI binds there
        AppAssemblies.Server, appPath: _environment.ContentRootPath).Initialize();

    services.AddRouting();
}
```

The endpoint is covered in [04](04-aspnetcore-endpoint.md). Use cases called locally (the data portal resolves them
from the request scope) need no connection setup: the local endpoint is implied.

### 7.2 ASP.NET server with a web UI (`Client | Server`)

Same as a server, plus the UI. The UI's scopes (a Blazor Server circuit, an MVC request) use the local endpoint by
default. If the UI must call a *separate* application server instead, configure it:

```json
"DataPortalConnectionConfiguration": { "Transport": "http", "Address": "https://app-server.acme.local/api/dataportal" }
```

Every scope without an explicit `Initialize` then calls that server, while the endpoint this host maps still
serves its own remote clients.

### 7.3 Thin client (`Client`, Blazor WebAssembly)

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
new AcmeIoCManager(builder.Services, builder.Configuration, BuildConfiguration.Client,
    builder.HostEnvironment.IsDevelopment(), BindingMode.Wpf, AppAssemblies.ThinClient,
    httpHandlerFactory: null).Initialize();  // a browser's handler cannot be replaced
```

- Without a configured endpoint, every scope must `Initialize` its connection (e.g. at login). With one (e.g.
  `"Address": "https://acme.example/api/dataportal"`, often the host the app was served from), nothing else is needed.
- `CreateHttpMessageHandler` must return `null`.
- Plugins cannot be loaded from a folder in a browser (see [01 §14](01-data-portal-mechanics.md#14-known-limitations)).
- Blazor WebAssembly runs as one user per container; a scope per login is still the clean way to switch server or
  tenant.

### 7.4 Thick client (`Client | Local`, WinForms/WPF)

```csharp
var services = new ServiceCollection();
services.AddLogging();
new AcmeIoCManager(services, configuration, BuildConfiguration.Client | BuildConfiguration.Local,
    isDevelopment: false, BindingMode.WinForms, AppAssemblies.ThickClient,
    () => new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }).Initialize();
var root = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
```

Then one **session scope per login** (server or local, user, tenant): see
[03 §10](03-app-data-portal-components.md#10-client-sessions-connection-identity-tenant).

---

## 8. Configuration classes

```csharp
using A5Soft.CARMA.Application;
using A5Soft.CARMA.Domain;

public interface IMailConfiguration { string SmtpHost { get; } int Port { get; } }

public sealed class MailConfiguration : ConfigurationBase, IMailConfiguration    // derives DIRECTLY
{
    public override Type InterfaceType => typeof(IMailConfiguration);
    public override BuildConfiguration ForBuildConfiguration => BuildConfiguration.Server;   // a constant
    // public override string RootSection => "Mail";   // only to bind from another section than "MailConfiguration"

    public string SmtpHost { get; set; } = "localhost";
    public int Port { get; set; } = 25;

    public override void Init(string appPath, bool isDevelopment) { /* computed values, relative paths */ }
    public override void Validate()
    {
        if (Port < 1 || Port > 65535) throw new InvalidOperationException("MailConfiguration:Port is not valid.");
    }
}
```

- Found by the scan if the class derives **directly** from `ConfigurationBase` (a class deriving from another
  configuration is not registered) and has a public parameterless constructor.
- Registered as a **singleton instance** under `InterfaceType`; consumers depend on the interface.
- **Section** = `RootSection`, by default the class name (`"MailConfiguration"`). A missing section keeps every
  default.
- `ForBuildConfiguration` is read from a throw-away instance during the scan: return a constant.
- `Validate` failures stop startup. Validate everything there rather than failing on first use.
- Framework configurations: `DataPortalClientOptions`, `DataPortalServerOptions`, `HttpDataPortalClientOptions`
  (`Client`), `HttpDataPortalServerOptions` (`Server`), `DataPortalConnectionConfiguration`, `PluginsConfiguration`.
  See [05](05-configuration-reference.md).

---

## 9. Plugins

### 9.1 Model

- **Installed per app**, by folder: `<app path>/<PluginsConfiguration.Folder>/<plugin folder>/<library>`. Every
  sub-folder of the plugins folder is a plugin; there is no list to maintain.
- **Library name**: `PluginsConfiguration.LibraryName` (the same file name in every folder, e.g. `Plugin.dll`), or
  unset for a library named after its folder (`Acme.Invoicing/Acme.Invoicing.dll`).
- **Identity**: `[assembly: PluginId("Acme.Invoicing")]` is **mandatory**; it is the plugin's name in tenant
  assignments, identities and the fingerprint, the same on every machine whatever the folder is called (compared
  ignoring case).
- **One version per plugin per app.** A refused plugin always **stops startup**: no app runs with a broken plugin.
- A plugin can add new use cases, override app or framework services, override tenant scoped services for the
  tenants assigned to it, add transports, and contain UI for its use cases.
- Plugins use the **global** configuration only (sections in the app's configuration). Per-tenant settings belong in
  the tenant's database.

### 9.2 Plugin projects

`Directory.Build.props` in the plugins' source folder, so every plugin gets its id from its project name:

```xml
<Project>
  <ItemGroup>
    <AssemblyAttribute Include="A5Soft.CARMA.Domain.IoC.PluginIdAttribute">
      <_Parameter1>$(MSBuildProjectName)</_Parameter1>
    </AssemblyAttribute>
  </ItemGroup>
</Project>
```

A plugin project:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <!-- writes the .deps.json the load context resolves private dependencies with -->
    <EnableDynamicLoading>true</EnableDynamicLoading>
  </PropertyGroup>
  <ItemGroup>
    <!-- shared with the app: compile against them, never copy them into the plugin folder -->
    <ProjectReference Include="..\Acme.Core\Acme.Core.csproj">
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
  </ItemGroup>
</Project>
```

### 9.3 Loading

```csharp
public sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath) : base(Path.GetFileNameWithoutExtension(pluginPath))
        => _resolver = new AssemblyDependencyResolver(pluginPath);

    protected override Assembly Load(AssemblyName assemblyName)
    {
        // null: the default context resolves it, from the app (shared types stay the same types)
        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return null == path ? null : LoadFromAssemblyPath(path);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return null == path ? IntPtr.Zero : LoadUnmanagedDllFromPath(path);
    }
}
```

**The classic failure:** a copy of `A5Soft.CARMA.*.dll` or of an app contract assembly in a plugin folder is
loaded into the plugin's context, its types are *different types* from the app's, and the plugin's implementations
silently do not implement the app's interfaces (or fail with "cannot cast X to X"). Keep shared assemblies out of
plugin folders (`Private=false`, `ExcludeAssets=runtime`).

### 9.4 Validation at startup

Plugins are loaded first, all of them, then validated and added **in plugin id order** (the same order on every
machine, so the same plugin is refused everywhere). A plugin is refused (startup stops, the message lists every
violation) if:

- it breaches the data portal conventions (checked in depth, as no app test covers a plugin);
- one of its use cases or trusted wire types has a wire name already taken by another type;
- it overrides an app scoped service or a singleton that another plugin already overrides;
- it adds an implementation of a tenant scoped interface that allows multiple implementations;
- it overrides the same tenant scoped service twice;
- it is an assembly the app or another plugin already contains.

Native libraries of a plugin are resolved relative to its folder and loaded only once it is accepted. Then
`OnPluginAccepted(plugin)` is called.

### 9.5 Overrides

| Service | A plugin's implementation | Conflict |
|---|---|---|
| App scoped (scoped/transient) or singleton | replaces the app's/framework's **app wide** | two plugins → the second is refused |
| Tenant scoped | used for the tenants assigned the plugin; others keep the app's | allowed; those two plugins become **exclusive**: no tenant can be assigned both |
| `AllowMultiple` (not tenant scoped) | added beside the others | none |

### 9.6 System services from plugins

```csharp
protected override void AddSystemServices(IReadOnlyList<PluginAssembly> plugins, SystemServiceCollection services)
{
    services.AddInstance(typeof(IInstalledModules), new InstalledModules(plugins.Select(p => p.Id)));
    // or: services.Add<IReportCatalog, PluginReportCatalog>(ServiceLifetime.Singleton);
}
```

A system service cannot be overridden, must use the lifetime its `[Service]` declares, and cannot be tenant
scoped.

### 9.7 Client and server must have the same plugins

The plugin fingerprint (ids and assembly versions) is published in the server settings and sent on every call;
client and server refuse each other on a mismatch. Deploy plugins to every client and the server together.

---

## 10. Tenancy

An app that is not tenant based simply never uses `[TenantScoped]`: there is nothing to configure, and
`AddServiceFactory` is never called.

### 10.1 Declaring tenant scoped services

```csharp
[TenantScoped]
[Service(ServiceLifetime.Scoped)]
public interface IInvoiceNumbering { string Next(); }

[DefaultServiceImplementation(typeof(IInvoiceNumbering), BuildConfiguration.Backend)]
public sealed class DefaultInvoiceNumbering : IInvoiceNumbering { ... }
```

- Only scoped and transient services (a singleton is app wide; `[TenantScoped]` on a singleton stops startup).
- **Not inherited**: every use case interface states its own scope. A use case interface can be tenant scoped
  (`[TenantScoped][UseCase]`): it is then callable only with a tenant.
- **An app scoped service cannot depend on a tenant scoped one** (it could not be created in a scope without a
  tenant): checked at startup, every public constructor.
- A tenant scoped service may be **plugin-only** (no app default): only tenants assigned one of those plugins
  have it.

### 10.2 The tenant of a scope

`ITenantContext` (scoped) holds `TenantId` and `Plugins`, set **once** per scope with `Initialize(tenantId,
plugins)` (null tenant: a scope without a tenant). Reading it before `Initialize`, or resolving anything tenant
scoped before it, throws. Switching tenant = a new scope (*log out of the tenant, log into another* without
re-authenticating).

- Server: the server data portal initializes it on every call from the verified identity
  ([03 §9](03-app-data-portal-components.md#9-the-server-data-portal)).
- Client: the session initializes it from the identity it received.

Resolution: the container's factory for a tenant scoped interface calls `TenantServiceMap.Resolve`, which picks the
implementation of the one tenant plugin that overrides it, else the app's default, and resolves that implementation
from the same scope. No tenant → *"is tenant scoped, and the current scope has no tenant"*; tenant without it →
*"is not available for tenant ..."*.

### 10.3 Assigning plugins to tenants

Assignments live in the administrative database; the app implements `ITenantPluginProvider` (server side). They
are read **when the identity for a tenant is issued** (entering a tenant, refreshing the identity), checked with
`TenantServiceMap.ValidateAssignment`, and put into the identity as claims:

```csharp
[DefaultServiceImplementation(typeof(ITenantEntry), BuildConfiguration.Server)]
public sealed class TenantEntry : ITenantEntry
{
    // ctor: IAuthenticationStateProvider, ITenantPluginProvider, TenantServiceMap, ITokenService

    public async Task<string> EnterTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        var user = await _authentication.GetIdentityAsync();
        if (!user.IsAuthenticated) throw new UnauthorizedAccessException("Log in first.");
        // ... check that the user belongs to the tenant (administrative database) ...

        var assigned = await _tenantPlugins.GetPluginsAsync(tenantId, cancellationToken);
        // all installed, no two overriding the same tenant scoped service; throws otherwise
        var plugins = _tenantServices.ValidateAssignment(tenantId, assigned);

        var claims = user.Claims
            .Where(c => c.Type != AcmeServerDataPortal.TenantClaimType
                && c.Type != AcmeServerDataPortal.PluginClaimType
                && c.Type != DataPortalClientOptions.DefaultTokenClaimType)
            .Append(new Claim(AcmeServerDataPortal.TenantClaimType, tenantId))
            .Concat(plugins.Select(p => new Claim(AcmeServerDataPortal.PluginClaimType, p)))
            .ToList();

        // the client gets the token and opens a new session (scope) with the identity holding it
        return _tokens.Issue(new ClaimsIdentity(claims, user.AuthenticationType));
    }
}
```

Why in the identity and not per call: startup is synchronous, database access is asynchronous, and a per-call
lookup would cost a query on every call. The trade-off: an assignment change takes effect when the user enters the
tenant again (a removed plugin is detected per call and refused as "enter the tenant again").

---

## 11. The HTTP connection hook

`CreateHttpMessageHandler()` is the only place the HTTP transport's connection is configured; it is called once,
at the start of `Initialize`. Return `null` for the platform handler.

```csharp
protected override HttpMessageHandler CreateHttpMessageHandler() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(5),       // pick up DNS changes
    Expect100ContinueTimeout = TimeSpan.FromSeconds(5),       // let the server refuse uploads before they are sent
    // Proxy = new WebProxy("http://proxy:8080"), SslOptions = { ClientCertificates = ... }
};
```

- The handler is shared by every endpoint and owned by the transport (never disposed).
- Only the connection: a delegating handler must not add or change request headers (authorization, timeouts and
  headers belong to each call).
- **Once per process**: a second IoC manager in the same process (tests, a host running two builds) must return
  `null`, or `Initialize` throws.
- Not in a browser (throws `NotSupportedException`).

---

## 12. Services every app must provide

| Service | Builds | Why |
|---|---|---|
| `IAppVersionProvider` (singleton) | all | Client/server compatibility check; no default on purpose. |
| `IRemoteErrorMapper` (singleton) | all | Exceptions ⇄ remote errors. |
| `A5Soft.CARMA.Application.ILogger` (singleton) | all | Used by the server portal and the framework's services. |
| `IAuthenticationStateProvider` (scoped) | all | The current identity of the scope (use case bases, server portal). |
| The server data portal class | `Server` | Derives from `ServerDataPortalBase`; registered under its own type. |
| `ITenantPluginProvider` | `Server`, tenant-based apps | Plugin assignments. |
| Repository stubs | thin client | For use cases' backend dependencies (§3). |

See [03](03-app-data-portal-components.md) for each.

---

## 13. Testing the composition

1. **Build every deployment with validation** (one test per build):

   ```csharp
   [Theory]
   [InlineData(BuildConfiguration.Server)]
   [InlineData(BuildConfiguration.Client | BuildConfiguration.Server)]
   [InlineData(BuildConfiguration.Client)]
   [InlineData(BuildConfiguration.Client | BuildConfiguration.Local)]
   public void Composes(BuildConfiguration build)
   {
       var services = new ServiceCollection();
       services.AddLogging();
       new AcmeIoCManager(services, new ConfigurationBuilder().Build(), build, false, BindingMode.Wpf,
           AppAssemblies.For(build)).Initialize();
       using (services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true })) { }
   }
   ```

   `ValidateOnBuild` finds every missing dependency of every registered service, in each build.
2. **The data portal convention test** over every assembly declaring or implementing use cases
   ([03 §3.4](03-app-data-portal-components.md#34-the-convention-test)); it also reports singletons taking scoped or
   transient services.
3. In tests that create several IoC managers in one process, pass no HTTP handler (§11).

---

## 14. Pitfalls

| Pitfall | Effect | Avoid by |
|---|---|---|
| `ServiceLifetime` ambiguous between CARMA and Microsoft DI | CS0104 in the IoC manager | alias one (`using MsServiceLifetime = ...`) |
| `TryAdd` in `AddService` | only the first transport/health check registered; `local` or `http` "unknown" | always `Add` |
| Interface without `[Service]` | startup fails | mark every service interface |
| Use case implementation marked `Backend` | thin client cannot resolve the use case | `Any` for use cases; stubs for their backend dependencies |
| Repository stub assembly listed in the thick client | duplicate implementations, startup fails | stubs only in the thin client's `AppAssemblies` |
| Configuration deriving from another configuration class | not registered | derive directly from `ConfigurationBase` |
| `ForBuildConfiguration` computed from state | wrong builds | return a constant |
| `BindConfiguration` throwing on a missing section | startup fails for optional sections | `GetSection(...).Bind(...)` (no-op when missing) |
| Initializing the connection after something read it | `InvalidOperationException` | `Initialize` the connection first in a new scope |
| Two IoC managers setting the HTTP handler | second `Initialize` throws | handler only in the first, `null` elsewhere |
| Shared assemblies copied into a plugin folder | plugin types do not match the app's | `Private=false`, `ExcludeAssets=runtime` |
| Plugin without `PluginId` | startup fails | `Directory.Build.props` |
| Different plugins on client and server | every call: `VersionMismatch` | deploy plugins everywhere together |
| `[TenantScoped]` on a singleton, or an app scoped service depending on a tenant scoped one | startup fails | scoped/transient only; move the dependency |
| Expecting `[TenantScoped]` to be inherited | derived use case is app scoped | mark each interface |
| Singleton taking a scoped service | the scoped instance lives forever (MS DI catches scoped with `ValidateScopes`, not transient) | convention test; take `IServiceProvider` and resolve per use |
| `appPath` not set in ASP.NET | relative paths resolved against the binaries folder | pass `ContentRootPath` |
