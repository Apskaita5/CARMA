using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Login;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Session
{
    /// <summary>
    /// Ends every session of a user, e.g. after the user is disabled. Interface: <c>[AllowAdminOnly]</c>, or
    /// <c>[Authorize]</c> with the app's user management permission.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    public abstract class EndUserSessionsUseCaseBase<TAppContext, TIdentity> : SessionUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected EndUserSessionsUseCaseBase(TAppContext context, IAuthenticationServices services) : base(context, services) { }


        /// <summary>
        /// Ends every session of the user.
        /// </summary>
        /// <param name="userId">the user</param>
        public Task EndUserSessionsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

            return RunAsync(dp => dp.InvokeAsync(GetType(), userId),
                () => RequireServices().Store.EndUserSessionsAsync(userId, CancellationToken.None), userId);
        }
    }
}
