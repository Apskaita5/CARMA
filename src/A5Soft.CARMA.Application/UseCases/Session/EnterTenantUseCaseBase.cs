using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Login;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Session
{
    /// <summary>
    /// Enters a tenant: a new session for the tenant, with the user's permissions there and the tenant's plugins.
    /// Interface: <c>[Authorize]</c> without permissions.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    public abstract class EnterTenantUseCaseBase<TAppContext, TIdentity> : SessionUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected EnterTenantUseCaseBase(TAppContext context, IAuthenticationServices services) : base(context, services) { }


        /// <summary>
        /// Enters a tenant.
        /// </summary>
        /// <param name="tenantId">the tenant to enter</param>
        /// <returns>the identity of the new session</returns>
        public Task<TIdentity> EnterTenantAsync(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentNullException(nameof(tenantId));

            return RunAsync(dp => dp.FetchAsync<string, TIdentity>(GetType(), tenantId),
                () => EnterLocallyAsync(tenantId), tenantId);
        }


        /// <summary>
        /// Checks that the user may enter the tenant (e.g. by the administrative database); throws the app's
        /// <see cref="Authorization.Authorizers.NotAuthorizedException"/> if not.
        /// </summary>
        /// <param name="identity">the user's identity</param>
        /// <param name="tenantId">the tenant</param>
        protected abstract Task EnsureCanEnterAsync(TIdentity identity, string tenantId);


        private async Task<TIdentity> EnterLocallyAsync(string tenantId)
        {
            var services = RequireServices();
            var identity = Identity;

            await EnsureCanEnterAsync(identity, tenantId).ConfigureAwait(false);

            var assigned = null == services.TenantPlugins
                ? Array.Empty<string>()
                : await services.TenantPlugins.GetPluginsAsync(tenantId, CancellationToken.None).ConfigureAwait(false);
            var plugins = services.TenantServices.ValidateAssignment(tenantId, assigned);

            return await StartSessionAsync(services, identity.UserId, tenantId, plugins, false).ConfigureAwait(false);
        }
    }
}
