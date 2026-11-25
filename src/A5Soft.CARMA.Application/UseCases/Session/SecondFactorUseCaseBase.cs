using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Login;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Session
{
    /// <summary>
    /// Completes a login with the second factor: a new session, no longer waiting. Interface:
    /// <c>[AllowSecondFactorPending]</c>.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    public abstract class SecondFactorUseCaseBase<TAppContext, TIdentity> : SessionUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected SecondFactorUseCaseBase(TAppContext context, IAuthenticationServices services) : base(context, services) { }


        /// <summary>
        /// Provides the second factor.
        /// </summary>
        /// <param name="code">the code the user has received or generated</param>
        /// <returns>the identity of the new session; null if the code is wrong and attempts remain</returns>
        /// <exception cref="NotAuthenticatedException">no login is waiting, or too many wrong codes (the session is
        /// ended: log in again)</exception>
        public Task<TIdentity> ProvideSecondFactorAsync(string code)
            => RunAsync(dp => dp.FetchAsync<string, TIdentity>(GetType(), code), () => ProvideLocallyAsync(code));


        private async Task<TIdentity> ProvideLocallyAsync(string code)
        {
            var services = RequireServices();
            var identity = Identity;
            if (null == identity || !identity.IsSecondFactorPending) throw new NotAuthenticatedException();

            var ticketId = GetTicketId(services) ?? throw new NotAuthenticatedException();
            var key = "2fa:" + ticketId;
            var provider = services.SecondFactor ?? throw new InvalidOperationException(
                $"No {nameof(ISecondFactorProvider)} is registered.");

            if (string.IsNullOrWhiteSpace(code)
                || !await provider.VerifyCodeAsync(identity, code.Trim(), CancellationToken.None).ConfigureAwait(false))
            {
                if (services.Lockout.RecordFailure(key) < services.Configuration.SecondFactorMaxAttempts) return null;

                services.Lockout.RecordSuccess(key);
                await EndSessionAsync(services).ConfigureAwait(false);
                throw new NotAuthenticatedException();
            }

            services.Lockout.RecordSuccess(key);

            return await StartSessionAsync(services, identity.UserId, null, null, false).ConfigureAwait(false);
        }
    }
}
