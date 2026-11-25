using A5Soft.CARMA.Application.Authorization.Sessions;
using A5Soft.CARMA.Application.IoC.Plugins;
using A5Soft.CARMA.Application.Tenancy;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization.Login
{
    /// <summary>
    /// The server side services the authentication use cases (login, second factor, entering or leaving a tenant,
    /// logout) need, in one service registered in builds with the server role only: elsewhere the use cases receive
    /// null and go through the data portal.
    /// </summary>
    [Service(ServiceLifetime.Scoped)]
    public interface IAuthenticationServices
    {
        /// <summary>Gets the identity store.</summary>
        IIdentityStore Store { get; }

        /// <summary>Gets the app's identity provider.</summary>
        IIdentityProvider IdentityProvider { get; }

        /// <summary>Gets the security settings.</summary>
        ISecurityConfiguration Configuration { get; }

        /// <summary>Gets the lockout.</summary>
        ILoginLockout Lockout { get; }

        /// <summary>Gets the app's second factor provider; null if the app has none.</summary>
        ISecondFactorProvider SecondFactor { get; }

        /// <summary>Gets the app's tenant plugin assignments; null for an app that is not tenant based.</summary>
        ITenantPluginProvider TenantPlugins { get; }

        /// <summary>Gets the tenant scoped services, to validate a tenant's plugin assignment.</summary>
        TenantServiceMap TenantServices { get; }
    }
}
