using A5Soft.CARMA.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Authorization.Identity
{
    /// <summary>
    /// The identity of a scope: set once when the scope starts (by the app on a client, by the server data portal for a
    /// call, by the web middleware for a request), refreshed within the same session by the data portal.
    /// </summary>
    /// <remarks>
    /// <para>A scope belongs to one session: one user, one tenant, one data portal endpoint. Logging in, entering or
    /// leaving a tenant and logging out each start a new scope; a refresh only replaces the user data and the ticket.</para>
    /// <para>The framework's implementation, <see cref="Default.DefaultAuthenticationStateProvider"/>, is the one to
    /// derive from if an app needs another.</para>
    /// </remarks>
    [Service(ServiceLifetime.Scoped)]
    public interface IAuthenticationStateProvider
    {
        /// <summary>
        /// Gets the identity of the scope: never null; anonymous before <see cref="InitializeAsync"/> and before login.
        /// </summary>
        CarmaIdentity Identity { get; }

        /// <summary>
        /// Sets the identity of the scope, once, when the scope starts: before anything reads the identity, the data
        /// portal connection or the tenant.
        /// </summary>
        /// <param name="identity">the identity: a login result (with its endpoint set by
        /// <see cref="CarmaIdentity.WithEndpoint"/> on a client), or <see cref="CarmaIdentity.CreateAnonymous"/></param>
        /// <param name="verified">whether the identity has just been checked against the identity store for the whole
        /// life of the scope: the server data portal's call scopes; never set by an app</param>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        /// <exception cref="InvalidOperationException">the identity is already set</exception>
        Task InitializeAsync(CarmaIdentity identity, bool verified = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes sure that the identity is current where use cases run locally against an identity store (an ASP.NET
        /// server with a web UI): checks the ticket and refreshes the identity if the user data has changed. Does
        /// nothing in a scope that has no ticket, no identity store or has been verified already. Called by the
        /// authorization of every local use case call.
        /// </summary>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        /// <exception cref="NotAuthenticatedException">the session has ended</exception>
        Task EnsureCurrentAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces the identity with a refreshed one of the same session (user and tenant): called by the data portal.
        /// Subscribers are notified, without waiting for them, unless only the ticket has changed.
        /// </summary>
        /// <param name="refreshed">the refreshed identity; its endpoint is taken from the current identity</param>
        /// <returns>a task that completes when the identity is replaced, without waiting for the subscribers</returns>
        /// <exception cref="InvalidOperationException">the identity belongs to another session</exception>
        Task NotifyIdentityChangedAsync(CarmaIdentity refreshed);

        /// <summary>
        /// Subscribes to identity changes. A subscriber must not live longer than the scope; dispose the subscription
        /// when it goes away (a form closed, a component disposed). The handler runs through the
        /// <see cref="IUiDispatcher"/> if one is registered; a failing handler is logged.
        /// </summary>
        /// <param name="handler">the handler, given the old and the new identity</param>
        /// <returns>the subscription: dispose it to unsubscribe</returns>
        IDisposable Subscribe(Func<IdentityChangedEventArgs, Task> handler);
    }
}
