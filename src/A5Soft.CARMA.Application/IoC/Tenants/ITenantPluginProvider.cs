using A5Soft.CARMA.Domain;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.IoC.Tenants
{
    /// <summary>
    /// Gets the plugins assigned to a tenant, e.g. from the administrative database. Implemented by the app, server
    /// side only; used when the identity for a tenant is issued or refreshed, never per call.
    /// </summary>
    [Service(ServiceLifetime.Scoped)]
    public interface ITenantPluginProvider
    {
        /// <summary>
        /// Gets the plugins assigned to a tenant.
        /// </summary>
        /// <param name="tenantId">the tenant id</param>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        /// <returns>the identifiers (<see cref="PluginIdAttribute"/>) of the tenant's plugins; empty if it has none</returns>
        Task<IReadOnlyCollection<string>> GetPluginsAsync(string tenantId, CancellationToken cancellationToken);
    }
}
