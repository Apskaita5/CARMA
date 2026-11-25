using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Login;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Session
{
    /// <summary>
    /// Logs a user in: checks the credentials (the app's), applies the lockout, fetches the identity and starts a
    /// session, waiting for the second factor if one is required. Interface: <c>[AllowAnonymous]</c>.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TCredentials">the app's credentials</typeparam>
    public abstract class LoginUseCaseBase<TAppContext, TIdentity, TCredentials> : SessionUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected LoginUseCaseBase(TAppContext context, IAuthenticationServices services) : base(context, services) { }


        /// <summary>
        /// Logs the user in.
        /// </summary>
        /// <param name="credentials">the credentials the user has entered</param>
        /// <returns>the identity of the new session; waiting for the second factor if one is required
        /// (<see cref="CarmaIdentity.IsSecondFactorPending"/>)</returns>
        /// <exception cref="NotAuthenticatedException">wrong credentials, or locked out</exception>
        public Task<TIdentity> LoginAsync(TCredentials credentials)
        {
            if (null == credentials) throw new ArgumentNullException(nameof(credentials));

            // the user name only: credentials never reach the log
            return RunAsync(dp => dp.FetchAsync<TCredentials, TIdentity>(GetType(), credentials),
                () => LoginLocallyAsync(credentials), GetUserName(credentials));
        }


        /// <summary>
        /// Gets the user name of the credentials: the lockout key, and what is logged.
        /// </summary>
        /// <param name="credentials">the credentials</param>
        protected abstract string GetUserName(TCredentials credentials);

        /// <summary>
        /// Checks the credentials.
        /// </summary>
        /// <param name="credentials">the credentials</param>
        /// <returns>the user's id; null if the credentials are wrong</returns>
        protected abstract Task<string> AuthenticateAsync(TCredentials credentials);


        private async Task<TIdentity> LoginLocallyAsync(TCredentials credentials)
        {
            var services = RequireServices();
            var key = "login:" + (GetUserName(credentials) ?? string.Empty).Trim().ToLowerInvariant();

            if (IsLockedOut(services, key)) throw new NotAuthenticatedException();

            var userId = await AuthenticateAsync(credentials).ConfigureAwait(false);
            if (string.IsNullOrEmpty(userId))
            {
                services.Lockout.RecordFailure(key);
                throw new NotAuthenticatedException();
            }

            services.Lockout.RecordSuccess(key);

            var identity = await services.IdentityProvider.FetchAsync(userId, null, CancellationToken.None)
                .ConfigureAwait(false) ?? throw new NotAuthenticatedException();

            var pending = await IsSecondFactorRequiredAsync(services, identity).ConfigureAwait(false);
            var session = await StartSessionAsync(services, identity, null, null, pending).ConfigureAwait(false);

            if (pending) await services.SecondFactor.SendCodeAsync(session, CancellationToken.None).ConfigureAwait(false);

            return session;
        }

        /// <summary>
        /// Decides whether the user must provide a second factor, by <see cref="Authorization.Sessions.ISecurityConfiguration.SecondFactor"/>:
        /// never, always, for administrators, or as the app's <see cref="ISecondFactorProvider"/> decides.
        /// </summary>
        private static async Task<bool> IsSecondFactorRequiredAsync(IAuthenticationServices services, CarmaIdentity identity)
        {
            var policy = services.Configuration.SecondFactor;
            if (policy == SecondFactorPolicy.None) return false;

            var provider = services.SecondFactor ?? throw new InvalidOperationException(
                $"A second factor is configured ({policy}), but no {nameof(ISecondFactorProvider)} is registered.");

            return policy == SecondFactorPolicy.All
                || (policy == SecondFactorPolicy.Admins && (identity.IsAdmin || identity.IsTenantAdmin))
                || await provider.IsRequiredAsync(identity, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
