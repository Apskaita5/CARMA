using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Login;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Session
{
    /// <summary>
    /// Completes a password reset with the token sent by
    /// <see cref="RequestPasswordResetUseCaseBase{TAppContext,TIdentity,TRequest}"/>, and ends all of the user's
    /// sessions. The user then logs in with the new password. Interface: <c>[AllowAnonymous]</c>.
    /// </summary>
    /// <remarks>Wrong tokens count towards the lockout, per <see cref="GetLockoutKey"/>.</remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TConfirmation">the app's confirmation: the token and the new password</typeparam>
    public abstract class ConfirmPasswordResetUseCaseBase<TAppContext, TIdentity, TConfirmation> : SessionUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected ConfirmPasswordResetUseCaseBase(TAppContext context, IAuthenticationServices services)
            : base(context, services) { }


        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <param name="confirmation">the token and the new password</param>
        /// <returns>true if the password is reset; false if the token is not valid (wrong, expired, used) or the
        /// attempts are locked out</returns>
        public Task<bool> ConfirmPasswordResetAsync(TConfirmation confirmation)
        {
            if (null == confirmation) throw new ArgumentNullException(nameof(confirmation));

            return RunAsync(dp => dp.FetchAsync<TConfirmation, bool>(GetType(), confirmation), async () =>
            {
                var services = RequireServices();
                var key = "reset-confirm:" + (GetLockoutKey(confirmation) ?? string.Empty).Trim().ToLowerInvariant();

                if (IsLockedOut(services, key)) return false;

                var userId = await DoConfirmAsync(confirmation).ConfigureAwait(false);
                if (string.IsNullOrEmpty(userId))
                {
                    services.Lockout.RecordFailure(key);
                    return false;
                }

                services.Lockout.RecordSuccess(key);
                await services.Store.EndUserSessionsAsync(userId, CancellationToken.None).ConfigureAwait(false);

                return true;
            });
        }


        /// <summary>
        /// Gets the key wrong tokens are counted by, e.g. the email or the token's public id; never the secret.
        /// </summary>
        /// <param name="confirmation">the confirmation</param>
        protected abstract string GetLockoutKey(TConfirmation confirmation);

        /// <summary>
        /// Checks the token and, if it is valid, stores the new password and consumes the token. Throws the app's
        /// business exception for a new password that breaks its rules (the token stays valid).
        /// </summary>
        /// <param name="confirmation">the confirmation</param>
        /// <returns>the id of the user whose password is reset; null if the token is not valid</returns>
        protected abstract Task<string> DoConfirmAsync(TConfirmation confirmation);
    }
}
