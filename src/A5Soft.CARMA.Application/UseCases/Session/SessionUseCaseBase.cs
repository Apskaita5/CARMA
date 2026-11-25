using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Login;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Session
{
    /// <summary>
    /// The shared code of the use cases that start, end or change sessions and credentials. They run remotely
    /// through the data portal, or locally where the identity store is (a server, an ASP.NET server with a web UI).
    /// </summary>
    /// <remarks>
    /// <para>A use case that returns an identity starts a new session: the client starts a new scope with it
    /// (after <see cref="CarmaIdentity.WithEndpoint"/>) and discards the old one.</para>
    /// <para>None of them can be cancelled: they all change state. None logs its argument: credentials,
    /// codes and tokens never reach the log.</para>
    /// <para>The framework is not localized, so the app's hooks throw the app's own business exceptions; the
    /// bases throw <see cref="NotAuthenticatedException"/> only, which needs no message.</para>
    /// </remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    public abstract class SessionUseCaseBase<TAppContext, TIdentity> : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <summary>
        /// Creates the use case.
        /// </summary>
        /// <param name="context">the scope's application context</param>
        /// <param name="services">the server side services; null where they are not registered (a client)</param>
        protected SessionUseCaseBase(TAppContext context, IAuthenticationServices services) : base(context)
        {
            Services = services;
        }


        /// <summary>
        /// Gets the server side services; null where they are not registered (a client).
        /// </summary>
        protected IAuthenticationServices Services { get; }


        /// <summary>
        /// Gets the server side services, which a local run needs.
        /// </summary>
        /// <exception cref="NotSupportedException">they are not registered here</exception>
        protected IAuthenticationServices RequireServices() => Services ?? throw new NotSupportedException(
            $"{GetType().Name} runs where the identity store is, or remotely through the data portal.");

        /// <summary>
        /// Gets the ticket id of the scope's session; null if it has none (anonymous, a local login).
        /// </summary>
        /// <param name="services">the server side services</param>
        protected string GetTicketId(IAuthenticationServices services)
        {
            var ticket = Context.Authentication.Identity.Ticket;
            return null == ticket ? null : services.Store.ValidateTicket(ticket)?.TicketId;
        }

        /// <summary>
        /// Fetches the user's identity for a tenant and starts a session with it, ending the scope's session (if any).
        /// </summary>
        /// <param name="services">the server side services</param>
        /// <param name="userId">the user</param>
        /// <param name="tenantId">the tenant of the session; null for none</param>
        /// <param name="tenantPlugins">the plugins assigned to the tenant; null for none</param>
        /// <param name="secondFactorPending">whether the session waits for the second factor</param>
        /// <returns>the identity of the new session, holding its ticket</returns>
        /// <exception cref="NotAuthenticatedException">the identity provider does not know the user (any more)</exception>
        protected async Task<TIdentity> StartSessionAsync(IAuthenticationServices services, string userId,
            string tenantId, IReadOnlyCollection<string> tenantPlugins, bool secondFactorPending)
        {
            var identity = await services.IdentityProvider.FetchAsync(userId, tenantId, CancellationToken.None)
                .ConfigureAwait(false) ?? throw new NotAuthenticatedException();

            return await StartSessionAsync(services, identity, tenantId, tenantPlugins, secondFactorPending)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Starts a session with an identity fetched already, ending the scope's session (if any).
        /// </summary>
        /// <param name="services">the server side services</param>
        /// <param name="identity">the user's identity, fetched for the tenant</param>
        /// <param name="tenantId">the tenant of the session; null for none</param>
        /// <param name="tenantPlugins">the plugins assigned to the tenant; null for none</param>
        /// <param name="secondFactorPending">whether the session waits for the second factor</param>
        /// <returns>the identity of the new session, holding its ticket</returns>
        protected async Task<TIdentity> StartSessionAsync(IAuthenticationServices services, CarmaIdentity identity,
            string tenantId, IReadOnlyCollection<string> tenantPlugins, bool secondFactorPending)
        {
            if (null == identity) throw new ArgumentNullException(nameof(identity));

            await EndSessionAsync(services).ConfigureAwait(false);

            return (TIdentity)await services.Store.StartSessionAsync(identity, tenantId, tenantPlugins,
                secondFactorPending, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Ends the scope's session, if it has one.
        /// </summary>
        /// <param name="services">the server side services</param>
        protected async Task EndSessionAsync(IAuthenticationServices services)
        {
            var ticketId = GetTicketId(services);
            if (null != ticketId) await services.Store.EndSessionAsync(ticketId, CancellationToken.None)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a value indicating whether a lockout key is locked out (<see cref="ILoginLockout"/>).
        /// </summary>
        /// <param name="services">the server side services</param>
        /// <param name="key">the key</param>
        protected static bool IsLockedOut(IAuthenticationServices services, string key)
            => null != services.Lockout.GetLockout(key);
    }
}
