using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Login;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Session
{
    /// <summary>
    /// Requests a change of the current user's email: the app issues a token and sends it to the new address; the
    /// change is completed by <see cref="ConfirmEmailChangeUseCaseBase{TAppContext,TIdentity,TConfirmation}"/>.
    /// Interface: <c>[Authorize]</c> without permissions.
    /// </summary>
    /// <remarks>Rate limited by the lockout, per user: when the limit is reached, nothing is sent and nothing is
    /// said.</remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TRequest">the app's request: the new email, and the password if the app asks for it</typeparam>
    public abstract class RequestEmailChangeUseCaseBase<TAppContext, TIdentity, TRequest> : SessionUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected RequestEmailChangeUseCaseBase(TAppContext context, IAuthenticationServices services)
            : base(context, services) { }


        /// <summary>
        /// Requests the change.
        /// </summary>
        /// <param name="request">the new email, and the password if the app asks for it</param>
        public Task RequestEmailChangeAsync(TRequest request)
        {
            if (null == request) throw new ArgumentNullException(nameof(request));

            return RunAsync(dp => dp.InvokeAsync(GetType(), request), async () =>
            {
                var services = RequireServices();
                var identity = Identity;
                var key = "email-change:" + identity.UserId;

                if (IsLockedOut(services, key)) return;

                // every request counts: the lockout limits how many
                services.Lockout.RecordFailure(key);

                await DoRequestAsync(identity, request).ConfigureAwait(false);
            });
        }


        /// <summary>
        /// Checks the request (the address is valid and free; the password, if the app asks for it), issues a token
        /// and sends it to the new address (and, preferably, a notice to the old one). Throws the app's business
        /// exception for a request it refuses.
        /// </summary>
        /// <param name="identity">the current user</param>
        /// <param name="request">the request</param>
        protected abstract Task DoRequestAsync(TIdentity identity, TRequest request);
    }
}
