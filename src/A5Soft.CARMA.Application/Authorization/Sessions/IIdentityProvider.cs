using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization.Sessions
{
    /// <summary>
    /// Implemented by the app, for server builds, and for thick clients that publish application events; used at
    /// login, on entering or leaving a tenant, whenever the identity store rebuilds a session's identity, and for
    /// every background work item (<see cref="Background.IBackgroundWorkRunner"/>).
    /// </summary>
    /// <remarks>A singleton that may fetch any user's identity for any tenant: it must not depend on scoped services
    /// (e.g. a scoped repository, or anything tenant scoped), and opens its own data access (e.g. through a connection
    /// factory). It is called outside any user's scope: the session's identity is not known yet.</remarks>
    [Service(ServiceLifetime.Singleton)]
    public interface IIdentityProvider
    {
        /// <summary>
        /// Fetches the user's identity as it is now.
        /// </summary>
        /// <param name="userId">the user's identifier</param>
        /// <param name="tenantId">the tenant the identity is for (its permissions and tenant admin flag);
        /// null for none</param>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        /// <returns>the identity, with a stamp of its data; null if the user may no longer log in (disabled, removed
        /// from the tenant), which ends the session</returns>
        /// <remarks>should return null for deleted or disabled users</remarks>
        Task<CarmaIdentity> FetchAsync(string userId, string tenantId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates the system identity background work runs with: the app's identity type, an administrator
        /// (<see cref="CarmaIdentity.IsAdmin"/> true), with a user identifier and name that the audit trail shows
        /// (e.g. "system"). The framework appends the tenant and its plugins, and marks it
        /// <see cref="CarmaIdentity.IsSystem"/>.
        /// </summary>
        /// <returns>a new system identity</returns>
        CarmaIdentity CreateSystem();
    }
}
