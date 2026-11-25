using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Login;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Session
{
    /// <summary>
    /// Leaves the current tenant: a new session without a tenant. Interface: <c>[Authorize]</c> without permissions.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    public abstract class LeaveTenantUseCaseBase<TAppContext, TIdentity> : SessionUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected LeaveTenantUseCaseBase(TAppContext context, IAuthenticationServices services) : base(context, services) { }


        /// <summary>
        /// Leaves the current tenant.
        /// </summary>
        /// <returns>the identity of the new session</returns>
        public Task<TIdentity> LeaveTenantAsync() => RunAsync(dp => dp.FetchAsync<TIdentity>(GetType()),
            () => StartSessionAsync(RequireServices(), Identity.UserId, null, null, false));
    }
}
