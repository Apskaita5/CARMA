using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Application.IoC.Tenants
{
    /// <summary>
    /// The tenant scoped services and their implementations, and the rules for assigning plugins to tenants.
    /// Built by <see cref="IoCManagerBase"/> and added to the container. Immutable; thread safe. Empty in an app
    /// that is not tenant based.
    /// </summary>
    public sealed class TenantServiceMap
    {
        private readonly Dictionary<Type, TenantServiceInfo> _services;
        private readonly PluginCatalog _catalog;


        internal TenantServiceMap(IEnumerable<TenantServiceInfo> services, IEnumerable<ExclusivePlugins> exclusive,
            PluginCatalog catalog)
        {
            _services = services.ToDictionary(s => s.InterfaceType);
            Exclusive = exclusive.ToArray();
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }


        /// <summary>Gets the tenant scoped services.</summary>
        public IReadOnlyCollection<TenantServiceInfo> Services => _services.Values;

        /// <summary>Gets the plugin pairs no tenant can be assigned together.</summary>
        public IReadOnlyList<ExclusivePlugins> Exclusive { get; }


        /// <summary>Gets whether a service interface is tenant scoped.</summary>
        public bool IsTenantScoped(Type interfaceType) => _services.ContainsKey(interfaceType);

        /// <summary>
        /// Gets whether a service can be resolved in a scope: always for an app scoped service; for a tenant scoped
        /// one, only with a tenant that has an implementation.
        /// </summary>
        public bool IsAvailable(Type interfaceType, ITenantContext tenant)
            => !_services.TryGetValue(interfaceType, out var info) || null != info.For(tenant);

        /// <summary>Gets whether a tenant scoped service has the app's implementation, i.e. every tenant has it.</summary>
        public bool HasDefault(Type interfaceType)
            => _services.TryGetValue(interfaceType, out var info) && null != info.Default;

        /// <summary>
        /// Checks the plugins assigned to a tenant before they go into the identity: all installed, no exclusive
        /// pair. Call it wherever the identity for a tenant is issued or refreshed.
        /// </summary>
        /// <param name="tenantId">the tenant id, for the messages</param>
        /// <param name="pluginIds">the identifiers of the plugins assigned to the tenant</param>
        /// <returns>the plugin identifiers, each once</returns>
        /// <exception cref="InvalidOperationException">the assignment cannot be served; the message says why</exception>
        public IReadOnlyCollection<string> ValidateAssignment(string tenantId, IEnumerable<string> pluginIds)
        {
            var list = (pluginIds ?? Enumerable.Empty<string>()).Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

            var missing = list.Where(p => !_catalog.AreInstalled(new[] { p })).ToList();
            if (missing.Count > 0) throw new InvalidOperationException(
                $"Tenant {tenantId} is assigned plugins that are not installed: {string.Join(", ", missing)}.");

            var assigned = new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
            var clash = Exclusive.FirstOrDefault(e => assigned.Contains(e.First) && assigned.Contains(e.Second));
            if (null != clash) throw new InvalidOperationException($"Tenant {tenantId} is assigned {clash}.");

            return list;
        }

        /// <summary>
        /// Resolves a tenant scoped service for the scope's tenant: the container's factory for the interface.
        /// </summary>
        /// <param name="services">the provider of the current scope</param>
        /// <param name="interfaceType">the tenant scoped service interface</param>
        /// <exception cref="InvalidOperationException">the scope has no tenant (or it is not set yet), or the tenant
        /// does not have the service</exception>
        public object Resolve(IServiceProvider services, Type interfaceType)
        {
            if (null == services) throw new ArgumentNullException(nameof(services));

            var tenant = (ITenantContext)services.GetService(typeof(ITenantContext))
                ?? throw new InvalidOperationException($"{nameof(ITenantContext)} is not registered.");

            if (!_services.TryGetValue(interfaceType, out var registration))
                throw new InvalidOperationException($"{interfaceType.FullName} is not registered as tenant scoped.");

            var implementation = registration.For(tenant) ?? throw new InvalidOperationException(
                null == tenant.TenantId
                    ? $"{interfaceType.FullName} is tenant scoped, and the current scope has no tenant."
                    : $"{interfaceType.FullName} is not available for tenant {tenant.TenantId}.");

            return services.GetService(implementation.ImplementationType)
                ?? throw new InvalidOperationException(
                    $"{implementation.ImplementationType.FullName} (implementation of {interfaceType.FullName} " +
                    $"for tenant {tenant.TenantId}) is not registered in the container.");
        }
    }
}
