using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Login;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Session
{
    /// <summary>
    /// Requests a password reset: the app issues a token and sends it to the user (e.g. a link by email); the
    /// reset is completed by <see cref="ConfirmPasswordResetUseCaseBase{TAppContext,TIdentity,TConfirmation}"/>.
    /// Interface: <c>[AllowAnonymous]</c>.
    /// </summary>
    /// <remarks>The answer is the same whether the user exists or not, and whether the request is rate limited
    /// (the lockout, per <see cref="GetLockoutKey"/>) or not: nothing tells a caller who has an account.</remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TRequest">the app's request, e.g. the user's email</typeparam>
    public abstract class RequestPasswordResetUseCaseBase<TAppContext, TIdentity, TRequest> : SessionUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected RequestPasswordResetUseCaseBase(TAppContext context, IAuthenticationServices services)
            : base(context, services) { }


        /// <summary>
        /// Requests a password reset.
        /// </summary>
        /// <param name="request">the request, e.g. the user's email</param>
        public Task RequestPasswordResetAsync(TRequest request)
        {
            if (null == request) throw new ArgumentNullException(nameof(request));

            return RunAsync(dp => dp.InvokeAsync(GetType(), request), async () =>
            {
                var services = RequireServices();
                var key = "reset:" + (GetLockoutKey(request) ?? string.Empty).Trim().ToLowerInvariant();

                if (IsLockedOut(services, key)) return;

                // every request counts: the lockout limits how many
                services.Lockout.RecordFailure(key);

                await DoRequestAsync(request).ConfigureAwait(false);
            });
        }


        /// <summary>
        /// Gets the key the requests are rate limited by, e.g. the email.
        /// </summary>
        /// <param name="request">the request</param>
        protected abstract string GetLockoutKey(TRequest request);

        /// <summary>
        /// Finds the user; if there is one, issues a reset token and sends it. Does nothing for an unknown user,
        /// and should take about as long: the caller must not learn who has an account.
        /// </summary>
        /// <param name="request">the request</param>
        protected abstract Task DoRequestAsync(TRequest request);
    }
}
