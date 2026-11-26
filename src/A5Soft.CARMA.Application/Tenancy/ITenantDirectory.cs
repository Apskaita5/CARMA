using A5Soft.CARMA.Domain;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Tenancy
{
    /// <summary>
    /// Tells the active tenants, e.g. from the administrative database. Implemented by a tenant based app, server
    /// side only: a cron job that runs per tenant (<see cref="Background.ICronJob"/>) runs once for each.
    /// </summary>
    /// <remarks>Its presence is what makes the background runner treat the app as tenant based: without it every cron
    /// job runs once, without a tenant. A tenant based app with tenant scoped services and per tenant cron jobs fails
    /// startup without it.</remarks>
    [Service(ServiceLifetime.Singleton)]
    public interface ITenantDirectory
    {
        /// <summary>
        /// Gets the tenants that background work runs for.
        /// </summary>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        /// <returns>the identifiers of the active tenants</returns>
        Task<IReadOnlyCollection<string>> GetActiveTenantsAsync(CancellationToken cancellationToken);
    }
}
