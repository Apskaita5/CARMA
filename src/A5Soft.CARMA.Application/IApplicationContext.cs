using A5Soft.CARMA.Application.Authorization.Authorizers;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A container interface for app wide settings and services.
    /// </summary>
    /// <remarks>Inherit the interface and implement the container subject to the app service requirements.</remarks>
    [Service(ServiceLifetime.Scoped)]
    public interface IApplicationContext : Domain.IDomainContext
    {
        /// <summary>
        /// Gets the authorization rules of the use cases.
        /// </summary>
        IAuthorizationProvider AuthorizationProvider { get; }

        /// <summary>
        /// Gets the client data portal for remote use case invocations.
        /// </summary>
        IClientDataPortal DataPortal { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Gets the scope's identity.
        /// </summary>
        IAuthenticationStateProvider Authentication { get; }

        /// <summary>
        /// Gets the lookup service; null if the app has none.
        /// </summary>
        ILookupService Lookups { get; }

        /// <summary>
        /// Gets the app's factory of the business exceptions the use case boundary needs.
        /// </summary>
        IBusinessExceptionFactory BusinessExceptions { get; }
    }
}
