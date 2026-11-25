using System;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization.Authorizers
{
    /// <summary>
    /// Provides the authorizers of the use cases.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IAuthorizationProvider
    {
        /// <summary>
        /// Gets the authorizer of a use case.
        /// </summary>
        /// <param name="useCaseType">the use case interface, or an implementation (its interface carrying the
        /// authorization attributes is used)</param>
        IUseCaseAuthorizer GetAuthorizer(Type useCaseType);
    }
}