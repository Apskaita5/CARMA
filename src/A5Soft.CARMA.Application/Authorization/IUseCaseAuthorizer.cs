using System;
using System.Security.Claims;

namespace A5Soft.CARMA.Application.Authorization
{
    /// <summary>
    /// A base interface for authorization (engine) method for a particular use case type..
    /// </summary>
    public interface IUseCaseAuthorizer
    {

        /// <summary>
        /// Gets a type of a use case that the authorizer is for.
        /// </summary>
        Type UseCaseType { get; }

        /// <summary>
        /// Returns true if the user identified by the identity is authorized to invoke use case
        /// (i.e. whether the user is authorized to invoke a use case at least for some parameters);
        /// otherwise returns false or throws a not authorized exception if specified by the parameter throwOnUnauthorized. 
        /// </summary>
        /// <param name="identity">user identity</param>
        /// <param name="throwOnUnauthorized">whether to throw an application specific not authorized exception
        /// if the user is not authorized</param>
        bool IsAuthorized(ClaimsIdentity identity, bool throwOnUnauthorized = false);
                  
    }
}