using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization.Sessions
{
    /// <summary>
    /// The server's sessions: issues and validates tickets, and keeps the current identity of every session by its
    /// ticket identifier. A session lives from login to logout; a ticket identifier unknown to the store means the
    /// session has ended (logout, expiry, a server restart with the default store): the caller is unauthenticated.
    /// </summary>
    /// <remarks>
    /// <para>The framework's <see cref="Default.DefaultIdentityStore"/> keeps everything in memory, signing keys
    /// included: a single server, and every user logs in again after a restart. An app with several servers, or that
    /// must survive restarts, implements its own (shared cache, persistent keys).</para>
    /// <para>Sliding expiration (<see cref="ISecurityConfiguration.SlidingExpiration"/>) issues a new ticket string,
    /// with the same identifier, on every call; a changed identity also gets a new ticket string, with its new stamp.</para>
    /// </remarks>
    [Service(ServiceLifetime.Singleton)]
    public interface IIdentityStore
    {
        /// <summary>
        /// Validates a ticket without the cache: signature and expiry.
        /// </summary>
        /// <param name="ticket">the ticket received; untrusted</param>
        /// <returns>what the ticket says; null if it is forged, malformed or expired</returns>
        TicketInfo ValidateTicket(string ticket);

        /// <summary>
        /// Gets the current identity of a session, corrected (fetched again) if it has been invalidated, with its current
        /// ticket.
        /// </summary>
        /// <param name="ticket">a validated ticket</param>
        /// /// <param name="identityProvider">the app's identity provider, to fetch the identity again for a correction</param>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        /// <returns>the identity; null if the session has ended</returns>
        Task<CarmaIdentity> FindAsync(TicketInfo ticket, IIdentityProvider identityProvider,
            CancellationToken cancellationToken);

        /// <summary>
        /// Starts a session (login, entering or leaving a tenant, second factor provided).
        /// </summary>
        /// <param name="identity">the user's identity as fetched for the tenant</param>
        /// <param name="tenantId">the tenant of the session; null for none</param>
        /// <param name="tenantPlugins">the plugins assigned to the tenant, validated</param>
        /// <param name="secondFactorPending">whether the second factor is still to be provided: a short session that
        /// can only provide it or log out</param>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        /// <returns>the identity of the new session, holding its ticket</returns>
        Task<CarmaIdentity> StartSessionAsync(CarmaIdentity identity, string tenantId,
            IReadOnlyCollection<string> tenantPlugins, bool secondFactorPending, CancellationToken cancellationToken);

        /// <summary>Ends a session (logout).</summary>
        Task EndSessionAsync(string ticketId, CancellationToken cancellationToken);

        /// <summary>Ends every session of a user (log out everywhere, user deleted).</summary>
        Task EndUserSessionsAsync(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Marks a user's sessions for correction: their identities are fetched again before next use. Call it after
        /// any change to the user's security.
        /// </summary>
        Task InvalidateUserAsync(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Marks every session for correction: e.g. after a role change.
        /// </summary>
        Task InvalidateAllAsync(CancellationToken cancellationToken);
    }
}
