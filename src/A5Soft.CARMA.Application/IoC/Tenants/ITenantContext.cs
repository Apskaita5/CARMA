using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.IoC.Tenants
{
    /// <summary>
    /// The tenant of the current scope and the plugins assigned to it, fixed when the scope is created: the server
    /// data portal sets it for each call from the verified identity; a client sets it for each scope it creates from
    /// the identity it received (switching tenant creates a new scope).
    /// </summary>
    [Service(ServiceLifetime.Scoped)]
    public interface ITenantContext
    {
        /// <summary>Gets whether the tenant of this scope has been set.</summary>
        bool IsInitialized { get; }

        /// <summary>Gets the tenant id of this scope; null if the scope has no tenant.</summary>
        /// <exception cref="InvalidOperationException">not initialized yet</exception>
        string TenantId { get; }

        /// <summary>Gets the identifiers of the plugins assigned to the tenant; empty without a tenant.</summary>
        /// <exception cref="InvalidOperationException">not initialized yet</exception>
        IReadOnlyCollection<string> Plugins { get; }

        /// <summary>Sets the tenant of this scope, once.</summary>
        /// <param name="tenantId">the tenant id; null for a scope without a tenant</param>
        /// <param name="plugins">the identifiers of the plugins assigned to the tenant; ignored without a tenant</param>
        /// <exception cref="InvalidOperationException">already initialized</exception>
        void Initialize(string tenantId, IEnumerable<string> plugins);
    }
}
