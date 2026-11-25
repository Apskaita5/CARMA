using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Login;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Session
{
    /// <summary>
    /// Completes a change of email with the token sent by
    /// <see cref="RequestEmailChangeUseCaseBase{TAppContext,TIdentity,TRequest}"/>. The user's sessions get the new
    /// identity on their next call. Interface: <c>[AllowAnonymous]</c>: the link may be opened where the user is not
    /// logged in.
    /// </summary>
    /// <remarks>The app must change the user's security stamp with the email, so that the sessions see the change.
    /// Wrong tokens count towards the lockout, per <see cref="GetLockoutKey"/>.</remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TConfirmation">the app's confirmation: the token</typeparam>
    public abstract class ConfirmEmailChangeUseCaseBase<TAppContext, TIdentity, TConfirmation> : SessionUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected ConfirmEmailChangeUseCaseBase(TAppContext context, IAuthenticationServices services)
            : base(context, services) { }


        /// <summary>
        /// Changes the email.
        /// </summary>
        /// <param name="confirmation">the token</param>
        /// <returns>true if the email is changed; false if the token is not valid (wrong, expired, used) or the
        /// attempts are locked out</returns>
        public Task<bool> ConfirmEmailChangeAsync(TConfirmation confirmation)
        {
            if (null == confirmation) throw new ArgumentNullException(nameof(confirmation));

            return RunAsync(dp => dp.FetchAsync<TConfirmation, bool>(GetType(), confirmation), async () =>
            {
                var services = RequireServices();
                var key = "email-confirm:" + (GetLockoutKey(confirmation) ?? string.Empty).Trim().ToLowerInvariant();

                if (IsLockedOut(services, key)) return false;

                var userId = await DoConfirmAsync(confirmation).ConfigureAwait(false);
                if (string.IsNullOrEmpty(userId))
                {
                    services.Lockout.RecordFailure(key);
                    return false;
                }

                services.Lockout.RecordSuccess(key);
                await services.Store.InvalidateUserAsync(userId, CancellationToken.None).ConfigureAwait(false);

                return true;
            });
        }


        /// <summary>
        /// Gets the key wrong tokens are counted by, e.g. the token's public id; never the secret.
        /// </summary>
        /// <param name="confirmation">the confirmation</param>
        protected abstract string GetLockoutKey(TConfirmation confirmation);

        /// <summary>
        /// Checks the token and, if it is valid, stores the new email, changes the security stamp and consumes the
        /// token. Throws the app's business exception if the address has been taken meanwhile.
        /// </summary>
        /// <param name="confirmation">the confirmation</param>
        /// <returns>the id of the user whose email is changed; null if the token is not valid</returns>
        protected abstract Task<string> DoConfirmAsync(TConfirmation confirmation);
    }
}
