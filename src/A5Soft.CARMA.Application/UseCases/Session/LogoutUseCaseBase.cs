using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Login;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Session
{
    /// <summary>
    /// Logs out: ends the session. The client then starts a new, anonymous scope. Interface:
    /// <c>[AllowSecondFactorPending]</c>, so that a login waiting for the second factor can be abandoned.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    public abstract class LogoutUseCaseBase<TAppContext, TIdentity> : SessionUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected LogoutUseCaseBase(TAppContext context, IAuthenticationServices services) : base(context, services) { }


        /// <summary>
        /// Logs out. A session that has ended already is not an error.
        /// </summary>
        public Task LogoutAsync() => RunAsync(async dp =>
        {
            try
            {
                await dp.InvokeAsync(GetType()).ConfigureAwait(false);
            }
            catch (NotAuthenticatedException)
            {
                // the session has ended already
            }
        }, async () =>
        {
            // a local login (a thick client) has no session to end
            if (null == Context.Authentication.Identity.Ticket) return;
            await EndSessionAsync(RequireServices()).ConfigureAwait(false);
        });
    }
}
