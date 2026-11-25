using A5Soft.CARMA.Application.Authorization.Authorizers;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Application.DataPortal.Protocol;
using A5Soft.CARMA.Application.IoC.Plugins;
using A5Soft.CARMA.Application.Navigation;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.IoC;
using A5Soft.CARMA.Domain.UnitTests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;

namespace A5Soft.CARMA.Application.IoC
{
    /// <summary>
    /// Configures the IoC container from the app's assemblies and plugins: services, the remote use case
    /// registry, configuration and unmanaged assemblies, all from one scan of each assembly.
    /// </summary>
    /// <remarks>
    /// <para>The app derives from it and supplies only what depends on its host and runtime:</para>
    /// <list type="bullet">
    /// <item><see cref="AppAssemblies"/>: the app's own assemblies (the framework's are added automatically);</item>
    /// <item><see cref="AddService"/>: adds one service to the container;</item>
    /// <item><see cref="BindConfiguration"/>: fills a configuration from the app configuration (e.g.
    /// appsettings.json);</item>
    /// <item><see cref="IsDevelopment"/>, and optionally <see cref="AppPath"/>;</item>
    /// <item><see cref="LoadUnmanagedAssembly"/>: loads a native library (e.g. with <c>NativeLibrary.Load</c>);
    /// only called if an assembly declares one;</item>
    /// /// <item>optionally <see cref="CreateHttpMessageHandler"/>: the connection settings of the HTTP data portal
    /// transport, the only place they can be set.</item>
    /// </list>
    /// <para>Loading assemblies and native libraries is left to the app because neither
    /// <c>AssemblyLoadContext</c> nor <c>NativeLibrary</c> is part of .NET Standard 2.1.</para>
    /// <para><see cref="Initialize"/> then runs once, at startup:</para>
    /// <list type="number">
    /// <item>scans the framework's and the app's assemblies and merges their remote services and trusted wire types
    /// (two different types with one wire name fail);</item>
    /// <item>loads the unmanaged assemblies the app's assemblies declare, relative to <see cref="AppPath"/>;</item>
    /// <item>composes the services: a service with more than one default implementation fails; a framework default is
    /// used only where the app does not replace it;</item>
    /// <item>loads every plugin folder (<see cref="IPluginsConfiguration"/>, <see cref="LoadPlugin"/>), then, in plugin
    /// identifier order, validates each plugin (<see cref="DataPortalConventions"/>, wire names, overrides) and adds
    /// it; any violation refuses the plugin, which stops startup. An override of an app scoped service or a singleton
    /// applies app wide (one plugin per service); an override of a tenant scoped service applies to the tenants
    /// assigned the plugin;</item>
    /// <item>checks that no app scoped service depends on a tenant scoped one;</item>
    /// <item>creates every configuration for this build, binds it, initializes and validates it;</item>
    /// <item>builds the remote use case registry, the plugin catalog and the tenant service map, and adds every
    /// service through <see cref="AddService"/>; each tenant scoped interface through
    /// <see cref="AddServiceFactory"/>, choosing the implementation for the scope's tenant.</item>
    /// </list>
    /// <para>Nothing of the app's own assemblies is validated against the data portal conventions at startup:
    /// that belongs in a unit test (<see cref="DataPortalConventions.Validate(Assembly)"/>).</para>
    /// </remarks>
    public abstract class IoCManagerBase
    {
        private readonly List<PluginAssembly> _plugins = new List<PluginAssembly>();
        private List<ApplicationServiceInfo> _services = new List<ApplicationServiceInfo>();
        private List<TenantServiceInfo> _tenantServices = new List<TenantServiceInfo>();
        private bool _initialized;


        /// <summary>
        /// Creates a new manager.
        /// </summary>
        /// <param name="buildConfiguration">the build configuration of the app: <see cref="BuildConfiguration.Client"/>
        /// or <see cref="BuildConfiguration.Server"/></param>
        /// <param name="isDevelopment">whether the app runs in the development environment; passed to
        /// <see cref="ConfigurationBase.Init"/></param>
        /// <param name="bindingMode">the binding mode of the app's UI framework (e.g. WinForms, WPF, Blazor), for
        /// every domain object the app creates (<see cref="IDomainSettings"/>)</param>
        /// <param name="appPath">the app base path: the app's unmanaged assembly paths are relative to it, and it is
        /// passed to <see cref="ConfigurationBase.Init"/>; null for <see cref="AppContext.BaseDirectory"/></param>
        /// <exception cref="ArgumentException">the build configuration is <see cref="BuildConfiguration.Any"/>, or the
        /// app path is not a valid path</exception>
        /// <exception cref="DirectoryNotFoundException">the app path does not exist</exception>
        protected IoCManagerBase(BuildConfiguration buildConfiguration, bool isDevelopment, BindingMode bindingMode,
            string appPath = null)
        {
            if (!buildConfiguration.IsAppBuild()) throw new ArgumentException(
                $"The build configuration {buildConfiguration} is not an app build: use Server, Client | Server, " +
                "Client or Client | Local.", nameof(buildConfiguration));

            BuildConfiguration = buildConfiguration;
            IsDevelopment = isDevelopment;
            BindingMode = bindingMode;
            AppPath = Path.GetFullPath(string.IsNullOrWhiteSpace(appPath) ? AppContext.BaseDirectory : appPath);

            if (!Directory.Exists(AppPath)) throw new DirectoryNotFoundException($"App path {AppPath} does not exist.");
        }

        #region Properties

        /// <summary>Gets the build configuration of the app.</summary>
        public BuildConfiguration BuildConfiguration { get; }

        /// <summary>
        /// Gets a value indicating whether the app runs in the development environment; passed to
        /// <see cref="ConfigurationBase.Init"/>.
        /// </summary>
        public bool IsDevelopment { get; }

        /// <summary>
        /// Gets the binding mode of the app's UI framework, for every domain object the app creates; added to the
        /// container as <see cref="IDomainSettings"/>.
        /// </summary>
        public BindingMode BindingMode { get; }

        /// <summary>
        /// Gets the app base path: the app's unmanaged assembly paths are relative to it, and it is passed to
        /// <see cref="ConfigurationBase.Init"/>. Default: <see cref="AppContext.BaseDirectory"/>.
        /// </summary>
        public string AppPath { get; }

        /// <summary>
        /// Gets every service added to the container, configurations and the registry included; empty before
        /// <see cref="Initialize"/>.
        /// </summary>
        public IReadOnlyList<ApplicationServiceInfo> Services => _services;

        /// <summary>
        /// Gets the tenant scoped services; empty before <see cref="Initialize"/> and in an app that is not tenant based.
        /// </summary>
        public IReadOnlyList<TenantServiceInfo> TenantServices => _tenantServices;

        /// <summary>Gets the plugins accepted.</summary>
        public IReadOnlyList<PluginAssembly> Plugins => _plugins;

        #endregion

        #region Implementation Overrides

        /// <summary>
        /// Gets the app's own assemblies to scan. The framework's (A5Soft.CARMA.Domain and
        /// A5Soft.CARMA.Application) are added automatically;
        /// plugins come from the plugin folders (<see cref="IPluginsConfiguration"/>).
        /// </summary>
        protected abstract IEnumerable<Assembly> AppAssemblies { get; }

        /// <summary>
        /// Loads a plugin library into the app, e.g. into its own <c>AssemblyLoadContext</c> resolving its dependencies
        /// from the plugin folder. Called only if the plugins folder contains plugins.
        /// </summary>
        /// <param name="libraryPath">the full path of the plugin library; the file exists</param>
        protected virtual Assembly LoadPlugin(string libraryPath)
            => throw new NotSupportedException(
                $"The plugins folder contains plugins: override {nameof(LoadPlugin)} to load them.");

        /// <summary>
        /// Called for each plugin once it is accepted, e.g. to scan it for the app's own plugin interfaces. Throwing stops
        /// startup, as for a refused plugin. Default: nothing.
        /// </summary>
        protected virtual void OnPluginAccepted(PluginAssembly plugin) { }

        /// <summary>
        /// Adds the app's system services, e.g. ones that depend on which plugins are loaded. Called once, after every
        /// plugin is accepted. A system service cannot be overridden: an interface that the framework, the app or a
        /// plugin implements as well stops startup (unless it allows multiple implementations); and it is app wide,
        /// never tenant scoped. Default: none.
        /// </summary>
        /// <param name="plugins">the plugins accepted, in identifier order</param>
        /// <param name="services">the collection to add the system services to</param>
        protected virtual void AddSystemServices(IReadOnlyList<PluginAssembly> plugins, SystemServiceCollection services) { }

        /// <summary>
        /// Creates the message handler every HTTP data portal call is sent through: the only place the app configures
        /// the HTTP connection, e.g. on .NET 5 a <c>SocketsHttpHandler</c> with a <c>PooledConnectionLifetime</c> (so
        /// that DNS changes are picked up), a longer <c>Expect100ContinueTimeout</c>, a proxy server or a client
        /// certificate. Called once, at the start of <see cref="Initialize"/>. Default: null, for the platform's handler.
        /// </summary>
        /// <returns>a new handler, owned by the HTTP transport from then on (never disposed); null for the platform's</returns>
        /// <remarks>Only the connection can be configured: headers, authorization and timeouts belong to each request
        /// and are set by the transport, so a delegating handler must not add or change any request header. Not in a
        /// browser (Blazor WebAssembly), whose own handler must stay. The handler is set once per process: a second IoC
        /// manager in the same process (e.g. in tests) must return null.</remarks>
        protected virtual HttpMessageHandler CreateHttpMessageHandler() => null;

        /// <summary>
        /// Adds a service to the container.
        /// </summary>
        /// <param name="service">the service: register <see cref="ApplicationServiceInfo.ImplementationInstance"/>
        /// as a singleton if it is set (configurations, the registry), otherwise
        /// <see cref="ApplicationServiceInfo.ImplementationType"/> with its lifetime, under
        /// <see cref="ApplicationServiceInfo.InterfaceType"/> (or the implementation type if there is none)</param>
        protected abstract void AddService(ApplicationServiceInfo service);

        /// <summary>
        /// Adds a service created by a factory, e.g. <c>services.Add(new ServiceDescriptor(type, factory, lifetime))</c>.
        /// Called only for tenant scoped services (each interface's factory chooses the implementation for the scope's
        /// tenant), so an app that is not tenant based does not need to override it.
        /// </summary>
        /// <param name="serviceType">the service interface</param>
        /// <param name="lifetime">scoped or transient</param>
        /// <param name="factory">creates the service from the provider of the current scope</param>
        protected virtual void AddServiceFactory(Type serviceType, ServiceLifetime lifetime,
            Func<IServiceProvider, object> factory)
            => throw new NotSupportedException(
                $"The app has tenant scoped services: override {nameof(AddServiceFactory)} to register them.");

        /// <summary>
        /// Fills a new configuration from the app configuration, e.g. with
        /// <c>configuration.GetSection(instance.RootSection).Bind(instance)</c>. Called before
        /// <see cref="ConfigurationBase.Init"/> and <see cref="ConfigurationBase.Validate"/>.
        /// </summary>
        protected abstract void BindConfiguration(ConfigurationBase configuration);

        /// <summary>
        /// Loads an unmanaged assembly, e.g. with <c>NativeLibrary.Load</c>.
        /// </summary>
        /// <param name="fullPath">the full path of the file, with the extension of the current platform
        /// (<see cref="ResolveUnmanagedPath"/>); the file exists</param>
        protected abstract void LoadUnmanagedAssembly(string fullPath);

        /// <summary>
        /// Resolves the path of an unmanaged assembly: the descriptor's path relative to the base path given,
        /// with the extension of the current platform (.dll, .so or .dylib). Any prefix (e.g. "lib") is part of
        /// the descriptor's path. Override to choose e.g. a subfolder per processor architecture.
        /// </summary>
        /// <param name="basePath">the app path, or the plugin folder</param>
        /// <param name="descriptor">the unmanaged assembly</param>
        protected virtual string ResolveUnmanagedPath(string basePath, UnmanagedAssemblyDescriptor descriptor)
        {
            string extension;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) extension = ".dll";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) extension = ".dylib";
            else extension = ".so";

            return Path.GetFullPath(Path.Combine(basePath, descriptor.Path + extension));
        }

        #endregion

        /// <summary>
        /// Scans the assemblies, loads and validates the plugins, and adds every service to the container through
        /// <see cref="AddService"/> and <see cref="AddServiceFactory"/>. Call once, at startup.
        /// </summary>
        /// <exception cref="InvalidOperationException">called twice; a service has more than one default implementation;
        /// two use cases or trusted wire types have the same name; an assembly cannot be scanned; a configuration cannot be
        /// created; a plugin has no identifier, shares one, or is refused (the message then lists the violations); or an
        /// app scoped service depends on a tenant scoped one</exception>
        /// <exception cref="FileNotFoundException">a declared unmanaged assembly or a plugin library does not exist</exception>
        public void Initialize()
        {
            if (_initialized) throw new InvalidOperationException("The IoC manager is already initialized.");

            if (null == AppAssemblies || !AppAssemblies.Any(a => null != a && !FrameworkAssemblies.Contains(a)))
                throw new InvalidOperationException("No app assemblies to init.");

            _initialized = true;

            // the HTTP connection settings, before anything could send a call
            var httpHandler = CreateHttpMessageHandler();
            if (null != httpHandler) DataPortal.Http.HttpDataPortalProxy.ConfigureHandler(httpHandler);

            var frameworkScans = FrameworkAssemblies.Select(AssemblyScan.Of).ToList();
            var appScans = AppAssemblies
                .Where(a => null != a && !FrameworkAssemblies.Contains(a))
                .Distinct()
                .Select(AssemblyScan.Of)
                .ToList();

            var names = new WireNames();
            var clashes = new List<string>();
            foreach (var scan in frameworkScans.Concat(appScans))
            {
                names.Merge(scan, (what, existing, type) => clashes.Add(RemoteUseCaseRegistry.ClashMessage(what, existing, type)));
            }
            if (clashes.Count > 0) throw new InvalidOperationException(string.Join(" ", clashes.Distinct()));

            // a menu part outside a plugin would never be used
            var strayMenus = frameworkScans.Concat(appScans).SelectMany(s => s.PluginMenuTypes).ToList();
            if (strayMenus.Count > 0) throw new InvalidOperationException(
                $"Only a plugin can have an {nameof(IPluginMenu)}; the app declares {string.Join(", ", strayMenus.Select(t => t.FullName))}. " +
                $"The app builds its menu in its {nameof(MainMenuProviderBase)}.");

            var identityType = GetIdentityType(appScans);

            foreach (var scan in appScans) LoadUnmanagedAssemblies(scan, AppPath);

            var composition = new ServiceComposition(Compose(frameworkScans, appScans));

            // the plugins configuration first: it says where the plugins are
            var pluginsService = CreateConfiguration(typeof(PluginsConfiguration));
            var pluginScans = AddPlugins((IPluginsConfiguration)pluginsService.ImplementationInstance,
                appScans, composition, names);

            var (services, tenantServices) = composition.Build();

            // the app's system services: after the plugins, so that none of them can replace one
            var system = new SystemServiceCollection();
            AddSystemServices(_plugins, system);
            AddSystemServicesTo(services, tenantServices, system.Services);

            ValidateDependencies(services, tenantServices);

            var configurations = frameworkScans.Concat(appScans).Concat(pluginScans)
                .SelectMany(s => s.GetConfigurations(BuildConfiguration))
                .Select(c => c.ImplementationType)
                .Where(t => t != typeof(PluginsConfiguration))
                .Distinct()
                .Select(CreateConfiguration)
                .Append(pluginsService)
                .ToList();

            var catalog = new PluginCatalog(_plugins);
            // permissions are declared by the app and its plugins only: the framework has none
            var permissions = PermissionCatalog.Create(
                appScans.SelectMany(s => s.PermissionTypes).Select(t => (t, (string)null))
                     .Concat(pluginScans.Zip(_plugins, (s, p) => (Scan: s, Plugin: p))
                         .SelectMany(x => x.Scan.PermissionTypes.Select(t => (t, x.Plugin.Id)))));
            var tenantMap = new TenantServiceMap(tenantServices, composition.Exclusive, catalog);

            services.AddRange(configurations);
            services.Add(Singleton(typeof(RemoteUseCaseRegistry),
                new RemoteUseCaseRegistry(names.UseCases, names.TrustedWireTypes, identityType)));
            services.Add(Singleton(typeof(PluginCatalog), catalog));
            services.Add(Singleton(typeof(TenantServiceMap), tenantMap));
            services.Add(Singleton(typeof(IDomainSettings), new DomainSettings(BindingMode)));
            services.Add(Singleton(typeof(IBuildInfo), new BuildInfo(BuildConfiguration)));
            services.Add(Singleton(typeof(PermissionCatalog), permissions));

            _services = services;
            _tenantServices = tenantServices;
            foreach (var service in services) AddService(service);

            // each implementation under its own type; the interface through a factory choosing the scope's
            foreach (var tenantService in tenantServices)
            {
                foreach (var implementation in tenantService.Implementations) AddService(implementation.AsSelf());

                var interfaceType = tenantService.InterfaceType;
                AddServiceFactory(interfaceType, tenantService.Lifetime, sp => tenantMap.Resolve(sp, interfaceType));
            }
        }

        #region Init Workflow Private Methods

        private static ApplicationServiceInfo Singleton(Type type, object instance)
            => new ApplicationServiceInfo(type, instance, ServiceLifetime.Singleton, false, BuildConfiguration.Any);

        private static Assembly[] FrameworkAssemblies
            => new[] { typeof(IDomainObject).Assembly, typeof(IoCManagerBase).Assembly }.Distinct().ToArray();

        /// <summary>
        /// Loads every plugin folder, checks the identifiers, then validates and adds the plugins in identifier order:
        /// the same on every machine whatever the folders are called, so the same plugin is refused everywhere.
        /// </summary>
        private List<AssemblyScan> AddPlugins(IPluginsConfiguration configuration, List<AssemblyScan> appScans,
            ServiceComposition composition, WireNames names)
        {
            var scans = new List<AssemblyScan>();
            if (!Directory.Exists(configuration.FullPath)) return scans;

            // 1. load every plugin folder
            var loaded = new List<PluginAssembly>();
            foreach (var folder in Directory.EnumerateDirectories(configuration.FullPath))
            {
                var libraryPath = configuration.GetLibraryPath(folder);
                if (!File.Exists(libraryPath)) throw new FileNotFoundException(
                    $"Plugin folder {folder} does not contain the plugin library.", libraryPath);

                var assembly = LoadPlugin(libraryPath) ?? throw new InvalidOperationException(
                    $"{nameof(LoadPlugin)} returned no assembly for {libraryPath}.");

                // the folders are the deployment: the same assembly twice is a deployment error, not something to skip
                if (FrameworkAssemblies.Contains(assembly) || appScans.Any(s => s.Source == assembly.FullName)
                    || loaded.Any(p => p.Assembly == assembly)) throw new InvalidOperationException(
                    $"Plugin {libraryPath} is an assembly the app or another plugin already contains.");

                loaded.Add(new PluginAssembly(assembly, folder));   // throws without a PluginId
            }

            // 2. identifiers unique
            var duplicate = loaded.GroupBy(p => p.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
            if (null != duplicate) throw new InvalidOperationException(
                $"Plugins {string.Join(" and ", duplicate)} have the same identifier {duplicate.Key}.");

            // 3. validate and add, in identifier order
            foreach (var plugin in loaded.OrderBy(p => p.Id, StringComparer.OrdinalIgnoreCase))
            {
                scans.Add(AddPlugin(plugin, composition, names));
                OnPluginAccepted(plugin);
            }

            return scans;
        }

        /// <summary>
        /// Validates a plugin and adds it; any violation refuses it, which stops startup.
        /// </summary>
        private AssemblyScan AddPlugin(PluginAssembly plugin, ServiceComposition composition, WireNames names)
        {
            var scan = AssemblyScan.Of(plugin.Assembly);

            // the data portal conventions in depth, as a plugin is not under the app's unit tests; then names and overrides
            var violations = DataPortalConventions.Validate(scan);
            foreach (var identityType in scan.IdentityTypes) violations.Add(
                new ConventionViolation(identityType, string.Empty,
                "is an identity type: only the app declares one, never a plugin"));

            names.Merge(scan, (what, existing, type) => violations.Add(new ConventionViolation(type, string.Empty,
                $"has the same name as {existing.AssemblyQualifiedName}, so the data portal cannot tell them apart")));
            violations.AddRange(composition.Add(plugin, scan.GetServices(BuildConfiguration)));

            var menu = CreatePluginMenu(scan, violations);

            if (violations.Count > 0) throw new InvalidOperationException(
                ConventionViolation.Format($"Plugin {plugin} is refused", violations));

            plugin.Menu = menu;

            // only an accepted plugin gets its native libraries loaded
            LoadUnmanagedAssemblies(scan, plugin.Folder);
            _plugins.Add(plugin);
            return scan;
        }

        /// <summary>
        /// Creates a plugin's part of the main menu from its scan: at most one <see cref="IPluginMenu"/>, with a public
        /// parameterless constructor.
        /// </summary>
        /// <returns>the menu part; null if the plugin has none, or it is not valid (added to the violations)</returns>
        private static IPluginMenu CreatePluginMenu(AssemblyScan scan, List<ConventionViolation> violations)
        {
            if (scan.PluginMenuTypes.Count < 1) return null;

            if (scan.PluginMenuTypes.Count > 1)
            {
                foreach (var type in scan.PluginMenuTypes) violations.Add(new ConventionViolation(type, string.Empty,
                    $"is one of {scan.PluginMenuTypes.Count} {nameof(IPluginMenu)} implementations: a plugin has at most one"));
                return null;
            }

            var menuType = scan.PluginMenuTypes[0];
            if (null == menuType.GetConstructor(Type.EmptyTypes))
            {
                violations.Add(new ConventionViolation(menuType, string.Empty,
                    "needs a public parameterless constructor: the framework creates it"));
                return null;
            }

            try
            {
                return (IPluginMenu)Activator.CreateInstance(menuType);
            }
            catch (TargetInvocationException ex)
            {
                violations.Add(new ConventionViolation(menuType, string.Empty,
                    $"cannot be created: its constructor throws {ex.InnerException?.GetType().Name}: {ex.InnerException?.Message}"));
                return null;
            }
        }

        /// <summary>
        /// Adds the system services; an interface already implemented (by the framework, the app, a plugin or another
        /// system service) stops startup, unless it allows multiple implementations.
        /// </summary>
        private static void AddSystemServicesTo(List<ApplicationServiceInfo> services,
            List<TenantServiceInfo> tenantServices, IReadOnlyList<ApplicationServiceInfo> systemServices)
        {
            var violations = new List<ConventionViolation>();

            foreach (var system in systemServices)
            {
                if (!system.AllowMultiple)
                {
                    var existing = services.Find(s => s.InterfaceType == system.InterfaceType);
                    var tenant = tenantServices.Find(t => t.InterfaceType == system.InterfaceType);
                    if (null != existing || null != tenant)
                    {
                        violations.Add(new ConventionViolation(system.ImplementationType, string.Empty,
                            $"is a system service for {system.InterfaceType}, which " +
                            $"{(existing?.ImplementationType ?? tenant.InterfaceType).FullName} already implements: " +
                            "a system service cannot be overridden"));
                        continue;
                    }
                }

                services.Add(system);
            }

            if (violations.Count > 0) throw new InvalidOperationException(
                ConventionViolation.Format("The system services cannot be added", violations));
        }

        /// <summary>
        /// An app scoped service cannot depend on a tenant scoped one: in a scope without a tenant it could not be
        /// created. Every public constructor is checked. Skipped when nothing is tenant scoped.
        /// </summary>
        private static void ValidateDependencies(List<ApplicationServiceInfo> services, List<TenantServiceInfo> tenantServices)
        {
            if (tenantServices.Count < 1) return;

            var tenantScoped = new HashSet<Type>(tenantServices.Select(t => t.InterfaceType));
            var violations = new List<ConventionViolation>();

            foreach (var service in services.Where(s => null == s.ImplementationInstance))
                foreach (var constructor in service.ImplementationType.GetConstructors())
                    foreach (var parameter in constructor.GetParameters())
                    {
                        var type = parameter.ParameterType;
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                            type = type.GetGenericArguments()[0];

                        if (tenantScoped.Contains(type)) violations.Add(new ConventionViolation(service.ImplementationType,
                            $"constructor parameter {parameter.Name}",
                            $"an app scoped service cannot depend on the tenant scoped {type}"));
                    }

            if (violations.Count > 0) throw new InvalidOperationException(
                ConventionViolation.Format("The services cannot be composed", violations));
        }

        /// <summary>
        /// The app's (and accepted plugins') services, with duplicates refused, then the framework defaults the
        /// app does not replace. Linear: duplicates are found through a dictionary.
        /// </summary>
        private List<ApplicationServiceInfo> Compose(IEnumerable<AssemblyScan> frameworkScans,
            IEnumerable<AssemblyScan> appScans)
        {
            var result = new List<ApplicationServiceInfo>();
            var singles = new Dictionary<(Type, bool), ApplicationServiceInfo>();
            var interfaces = new HashSet<Type>();

            foreach (var service in appScans.SelectMany(s => s.GetServices(BuildConfiguration)))
            {
                if (!service.AllowMultiple)
                {
                    // the same rule as ApplicationServiceInfo.IsDuplicateFor: by interface, or by implementation
                    // for a service registered without one
                    var key = (service.InterfaceType ?? service.ImplementationType, null == service.InterfaceType);
                    if (singles.TryGetValue(key, out var duplicate)) throw new InvalidOperationException(
                        $"Service {(service.InterfaceType ?? service.ImplementationType).FullName} has more than one " +
                        $"default implementation: {duplicate.ImplementationType.FullName} and {service.ImplementationType.FullName}.");
                    singles.Add(key, service);
                }

                interfaces.Add(service.InterfaceType);
                result.Add(service);
            }

            foreach (var service in frameworkScans.SelectMany(s => s.GetServices(BuildConfiguration)))
            {
                // a single service: the framework's default only where the app has none; a service with multiple
                // implementations (e.g. the data portal transports): the framework's are kept beside the app's
                if (service.AllowMultiple || null == service.InterfaceType || !interfaces.Contains(service.InterfaceType))
                    result.Add(service);
            }

            return result;
        }

        /// <summary>
        /// The app's identity type: exactly one, declared in the app assemblies, and serializable as the data portal
        /// needs it.
        /// </summary>
        /// <param name="appScans">the scans of the app assemblies</param>
        /// <returns>the identity type</returns>
        private static Type GetIdentityType(List<AssemblyScan> appScans)
        {
            var types = appScans.SelectMany(s => s.IdentityTypes).Distinct().ToList();
            if (types.Count != 1) throw new InvalidOperationException(types.Count < 1
                ? $"The app assemblies declare no identity type: derive exactly one class from {nameof(CarmaIdentity)}."
                : $"The app assemblies declare more than one identity type: {string.Join(", ", types.Select(t => t.FullName))}.");

            var defects = CarmaIdentity.GetDefects(types[0]);
            if (defects.Count > 0) throw new InvalidOperationException(
                $"Identity type {types[0].FullName} is not valid: {string.Join(" ", defects)}");

            return types[0];
        }

        private void LoadUnmanagedAssemblies(AssemblyScan scan, string basePath)
        {
            var descriptors = scan.ExternalDependencies?.UnmanagedAssemblies;
            if (null == descriptors) return;

            foreach (var descriptor in descriptors.Where(d => null != d
                && d.ForBuildConfiguration.Serves(BuildConfiguration)))
            {
                var path = ResolveUnmanagedPath(basePath, descriptor);
                if (!File.Exists(path)) throw new FileNotFoundException(
                    $"Unmanaged assembly {descriptor.Path} required by {scan.Source} was not found.", path);

                LoadUnmanagedAssembly(path);
            }
        }

        /// <summary>
        /// A new instance per manager: the instance the scan created only told its interface and build
        /// configuration, and is shared by everything scanning the assembly.
        /// </summary>
        private ApplicationServiceInfo CreateConfiguration(Type configurationType)
        {
            ConfigurationBase configuration;
            try
            {
                configuration = (ConfigurationBase)Activator.CreateInstance(configurationType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Configuration {configurationType.FullName} cannot be created.", ex);
            }

            BindConfiguration(configuration);
            configuration.Init(AppPath, IsDevelopment);
            configuration.Validate();

            return new ApplicationServiceInfo(configuration.InterfaceType, configuration, ServiceLifetime.Singleton,
                false, configuration.ForBuildConfiguration);
        }

        /// <summary>
        /// The remote services and trusted wire types of the scans merged so far, by wire name.
        /// </summary>
        private sealed class WireNames
        {
            public Dictionary<string, Type> UseCases { get; } = new Dictionary<string, Type>(StringComparer.Ordinal);
            public Dictionary<string, Type> TrustedWireTypes { get; } = new Dictionary<string, Type>(StringComparer.Ordinal);

            public void Merge(AssemblyScan scan, Action<string, Type, Type> onClash)
            {
                RemoteUseCaseRegistry.Merge(UseCases, scan.RemoteServices, (existing, type) => onClash("use cases", existing, type));
                RemoteUseCaseRegistry.Merge(TrustedWireTypes, scan.TrustedWireTypes, (existing, type) => onClash("types", existing, type));
            }
        }

        #endregion
    }
}
