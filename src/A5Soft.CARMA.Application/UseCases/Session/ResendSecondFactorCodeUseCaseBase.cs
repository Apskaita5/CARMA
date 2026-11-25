using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Login;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Session
{
    /// <summary>
    /// Sends the second factor code again, for a login waiting for it. Interface: <c>[AllowSecondFactorPending]</c>.
    /// </summary>
    /// <remarks>Rate limited by the lockout, per session: when the limit is reached, nothing is sent and nothing
    /// is said (the code already sent stays valid).</remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    public abstract class ResendSecondFactorCodeUseCaseBase<TAppContext, TIdentity> : SessionUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected ResendSecondFactorCodeUseCaseBase(TAppContext context, IAuthenticationServices services)
            : base(context, services) { }


        /// <summary>
        /// Sends the code again.
        /// </summary>
        /// <exception cref="NotAuthenticatedException">no login is waiting for the second factor</exception>
        public Task ResendCodeAsync() => RunAsync(dp => dp.InvokeAsync(GetType()), ResendLocallyAsync);


        private async Task ResendLocallyAsync()
        {
            var services = RequireServices();
            var identity = Identity;
            if (null == identity || !identity.IsSecondFactorPending) throw new NotAuthenticatedException();

            var key = "2fa-send:" + (GetTicketId(services) ?? throw new NotAuthenticatedException());
            if (IsLockedOut(services, key)) return;

            // every send counts: the lockout limits how many
            services.Lockout.RecordFailure(key);

            var provider = services.SecondFactor ?? throw new InvalidOperationException(
                $"No {nameof(ISecondFactorProvider)} is registered.");
            await provider.SendCodeAsync(identity, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
