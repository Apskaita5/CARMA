using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization.Sessions
{
    /// <summary>
    /// Fetches a user's identity as the app stores it: the user data only (the framework adds the session state).
    /// Implemented by the app, for server builds; used at login, on entering or leaving a tenant, and whenever the
    /// identity store rebuilds a session's identity.
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
        Task<CarmaIdentity> FetchAsync(string userId, string tenantId, CancellationToken cancellationToken);
    }
}
