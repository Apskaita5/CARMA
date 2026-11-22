using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Application.IoC.Tenants
{
    /// <summary>
    /// A tenant scoped service: the app's implementation and the plugins' overrides, chosen per scope from the
    /// plugins assigned to its tenant.
    /// </summary>
    public sealed class TenantServiceInfo
    {
        internal TenantServiceInfo(Type interfaceType, ServiceLifetime lifetime, ApplicationServiceInfo defaultService,
            IReadOnlyDictionary<string, ApplicationServiceInfo> overrides)
        {
            InterfaceType = interfaceType;
            Lifetime = lifetime;
            Default = defaultService;
            Overrides = overrides;
        }


        /// <summary>Gets the service interface.</summary>
        public Type InterfaceType { get; }

        /// <summary>Gets the lifetime: scoped or transient.</summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// Gets the app's implementation, for tenants without an override; null for a service only plugins add
        /// (only the tenants assigned one of them have it).
        /// </summary>
        public ApplicationServiceInfo Default { get; }

        /// <summary>Gets the plugins' implementations, by plugin identifier (case insensitive).</summary>
        public IReadOnlyDictionary<string, ApplicationServiceInfo> Overrides { get; }

        /// <summary>Gets every implementation, each once.</summary>
        public IEnumerable<ApplicationServiceInfo> Implementations
            => (null == Default ? Overrides.Values : Overrides.Values.Prepend(Default)).Distinct();


        /// <summary>
        /// Gets the implementation for a scope: the override of one of the tenant's plugins, else the default;
        /// null without a tenant, or if the tenant does not have the service.
        /// </summary>
        /// <exception cref="InvalidOperationException">two of the tenant's plugins override the service (an
        /// assignment the server refuses when it issues the identity)</exception>
        public ApplicationServiceInfo For(ITenantContext tenant)
        {
            if (null == tenant) throw new ArgumentNullException(nameof(tenant));
            if (null == tenant.TenantId) return null;

            ApplicationServiceInfo found = null;
            foreach (var plugin in tenant.Plugins)
            {
                if (!Overrides.TryGetValue(plugin, out var candidate)) continue;
                if (null != found) throw new InvalidOperationException(
                    $"Tenant {tenant.TenantId} has two plugins overriding {InterfaceType.FullName}.");
                found = candidate;
            }

            return found ?? Default;
        }
    }
}
