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
        /// Gets a type of use case that the authorizer is for.
        /// </summary>
        Type UseCaseType { get; }

        /// <summary>
        /// Returns true if the user identified by the identity is authorized to invoke use case without parameters
        /// (i.e. whether the user is authorized to invoke a use case at least for some parameters);
        /// otherwise returns false or throws a not authorized exception if specified by the parameter throwOnUnauthorized. 
        /// </summary>
        /// <param name="identity">user identity</param>
        /// <param name="throwOnUnauthorized">whether to throw an application specific not authorized exception
        /// if the user is not authorized</param>
        /// <returns></returns>
        bool IsAuthorized(ClaimsIdentity identity, bool throwOnUnauthorized = false);

        /// <summary>
        /// Returns true if the user identified by the identity is authorized to invoke use case
        /// with the given parameter (e.g. query criteria); otherwise returns false
        /// or throws a not authorized exception if specified by the parameter throwOnUnauthorized.
        /// </summary>
        /// <typeparam name="TParam">a type of the use case parameter to use for authorization</typeparam>
        /// <param name="identity">user identity</param>
        /// <param name="parameter">a use case parameter to use for authorization</param>
        /// <param name="throwOnUnauthorized">whether to throw an application specific not authorized exception
        /// if the user is not authorized</param>
        /// <returns></returns>
        bool IsAuthorized<TParam>(ClaimsIdentity identity, TParam parameter, bool throwOnUnauthorized = false);

        /// <summary>
        /// Returns value indicating whether an authorization method, that depends on
        /// a parameter (entity, criteria etc.) value, has been implemented for the parameter type specified.
        /// </summary>
        /// <typeparam name="TParam">a type of parameter that the authorization (might) depend on</typeparam>
        /// <returns>True, if an authorization method for the parameter type is implemented (exists);
        /// otherwise - false</returns>
        bool AuthorizationImplementedForParam<TParam>();

    }
}