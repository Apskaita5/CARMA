using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Login;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Session
{
    /// <summary>
    /// Changes the password of the current user, ends all of the user's sessions and starts a new one for the
    /// caller. Interface: <c>[Authorize]</c> without permissions.
    /// </summary>
    /// <remarks>Wrong current passwords count towards the lockout, per user: when it is reached, the session is
    /// ended (a stolen session must not be able to guess the password).</remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TRequest">the app's request: the current and the new password</typeparam>
    public abstract class ChangePasswordUseCaseBase<TAppContext, TIdentity, TRequest> : SessionUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected ChangePasswordUseCaseBase(TAppContext context, IAuthenticationServices services) : base(context, services) { }


        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="request">the current and the new password</param>
        /// <returns>the identity of the new session; null if the current password is wrong</returns>
        /// <exception cref="NotAuthenticatedException">too many wrong passwords: the session is ended</exception>
        public Task<TIdentity> ChangePasswordAsync(TRequest request)
        {
            if (null == request) throw new ArgumentNullException(nameof(request));

            return RunAsync(dp => dp.FetchAsync<TRequest, TIdentity>(GetType(), request),
                () => ChangeLocallyAsync(request));
        }


        /// <summary>
        /// Checks the current password and stores the new one. Throws the app's business exception for a new
        /// password that breaks its rules.
        /// </summary>
        /// <param name="identity">the current user</param>
        /// <param name="request">the current and the new password</param>
        /// <returns>false if the current password is wrong</returns>
        protected abstract Task<bool> TryChangePasswordAsync(TIdentity identity, TRequest request);


        private async Task<TIdentity> ChangeLocallyAsync(TRequest request)
        {
            var services = RequireServices();
            var identity = Identity;
            var key = "password:" + identity.UserId;

            if (IsLockedOut(services, key))
            {
                await EndSessionAsync(services).ConfigureAwait(false);
                throw new NotAuthenticatedException();
            }

            if (!await TryChangePasswordAsync(identity, request).ConfigureAwait(false))
            {
                services.Lockout.RecordFailure(key);
                if (!IsLockedOut(services, key)) return null;

                await EndSessionAsync(services).ConfigureAwait(false);
                throw new NotAuthenticatedException();
            }

            services.Lockout.RecordSuccess(key);

            // every session of the user ends, this one included; the caller continues in a new one
            await services.Store.EndUserSessionsAsync(identity.UserId, CancellationToken.None).ConfigureAwait(false);

            return await StartSessionAsync(services, identity.UserId, identity.TenantId, identity.TenantPlugins, false)
                .ConfigureAwait(false);
        }
    }
}
