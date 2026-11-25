using System;
using A5Soft.CARMA.Application.Authorization.Sessions;
using A5Soft.CARMA.Application.IoC.Plugins;
using A5Soft.CARMA.Application.Tenancy;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization.Login
{
    /// <inheritdoc />
    [DefaultServiceImplementation(typeof(IAuthenticationServices), BuildConfiguration.Server)]
    public sealed class AuthenticationServices : IAuthenticationServices
    {
        public AuthenticationServices(IIdentityStore store, IIdentityProvider identityProvider,
            ISecurityConfiguration configuration, ILoginLockout lockout, TenantServiceMap tenantServices,
            ISecondFactorProvider secondFactor = null, ITenantPluginProvider tenantPlugins = null)
        {
            Store = store ?? throw new ArgumentNullException(nameof(store));
            IdentityProvider = identityProvider ?? throw new ArgumentNullException(nameof(identityProvider));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Lockout = lockout ?? throw new ArgumentNullException(nameof(lockout));
            TenantServices = tenantServices ?? throw new ArgumentNullException(nameof(tenantServices));
            SecondFactor = secondFactor;
            TenantPlugins = tenantPlugins;
        }

        public IIdentityStore Store { get; }
        public IIdentityProvider IdentityProvider { get; }
        public ISecurityConfiguration Configuration { get; }
        public ILoginLockout Lockout { get; }
        public ISecondFactorProvider SecondFactor { get; }
        public ITenantPluginProvider TenantPlugins { get; }
        public TenantServiceMap TenantServices { get; }
    }
}
