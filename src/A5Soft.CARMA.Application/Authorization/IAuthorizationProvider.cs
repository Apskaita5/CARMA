using System;

namespace A5Soft.CARMA.Application.Authorization
{
    /// <summary>
    /// A base interface for authorization (engine) providers.
    /// </summary>
    public interface IAuthorizationProvider
    {
        /// <summary>
        /// Gets an authorizer for the use case type specified.
        /// </summary>
        /// <param name="useCaseType">use case type to get an authorizer for</param>
        /// <returns>an authorizer for the use case type specified</returns>
        IUseCaseAuthorizer GetAuthorizer(Type useCaseType);

        /// <summary>
        /// Gets an authorizer for the use case type specified.
        /// </summary>
        /// <typeparam name="T">use case type to get an authorizer for</typeparam>
        /// <returns>an authorizer for the use case type specified</returns>
        IUseCaseAuthorizer GetAuthorizer<T>();
    }
}