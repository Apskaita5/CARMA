using A5Soft.CARMA.Domain;
using System;
using System.Reflection;
using System.Security.Claims;

namespace A5Soft.CARMA.Application.Authorization.Default
{
    /// <summary>
    /// Authorization attribute for default authorization implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple =  false, Inherited = false)]
    public abstract class AuthorizationBaseAttribute : Attribute
    {
        private Func<string> _errorMessageResourceAccessor = null;
        private readonly string _errorMessageResourceName;
        private readonly Type _errorMessageResourceType;
                       

        /// <summary>
        /// Default constructor for any authorization attribute (unless derived authorization attribute overrides resource type)
        /// </summary>
        /// <param name="errorMessageResourceName">the resource name (property name) to use as the key for lookups on the resource type</param>
        /// <param name="errorMessageResourceType">the resource type to use for error message lookups</param>
        protected AuthorizationBaseAttribute(string errorMessageResourceName, Type errorMessageResourceType)
        {
            if (errorMessageResourceName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(errorMessageResourceName));
            
            _errorMessageResourceName = errorMessageResourceName;
            _errorMessageResourceType = errorMessageResourceType ?? 
                throw new ArgumentNullException(nameof(errorMessageResourceType)); ;
        }


        /// <summary>
        /// Returns true if the user identified by the identity is authorized to invoke use case of useCaseType
        /// without parameters (i.e. whether the user is authorized to invoke a use case at least for some parameters);
        /// otherwise returns false or throws a not authorized exception if specified by the parameter throwOnUnauthorized. 
        /// </summary>
        /// <param name="useCaseType">a type of the use case that is authorized</param>
        /// <param name="identity">user identity</param>
        /// <param name="throwOnUnauthorized">whether to throw an application specific not authorized exception
        /// if the user is not authorized</param>
        /// <param name="logger">a logger to use for authorization warnings</param>
        /// <returns></returns>
        public bool IsAuthorized(Type useCaseType, ClaimsIdentity identity,
            ILogger logger, bool throwOnUnauthorized = false)
        {
            EnsureValidAuthorizationParameters(useCaseType, identity, logger);

            return Authorize(identity, logger, throwOnUnauthorized);
        }

        /// <summary>
        /// Returns true if the user identified by the identity is authorized to invoke use case of useCaseType
        /// with the given parameter (e.g. query criteria); otherwise returns false
        /// or throws a not authorized exception if specified by the parameter throwOnUnauthorized.
        /// </summary>
        /// <typeparam name="TParam">a type of the use case parameter to use for authorization</typeparam>
        /// <param name="useCaseType">a type of the use case that is authorized</param>
        /// <param name="identity">user identity</param>
        /// <param name="parameter">a use case parameter to use for authorization</param>
        /// <param name="throwOnUnauthorized">whether to throw an application specific not authorized exception
        /// if the user is not authorized</param>
        /// <param name="logger">a logger to use for authorization warnings</param>
        /// <returns></returns>
        public bool IsAuthorized<TParam>(Type useCaseType, ClaimsIdentity identity, TParam parameter,
            ILogger logger, bool throwOnUnauthorized = false)
        {
            EnsureValidAuthorizationParameters(useCaseType, identity, logger);

            return Authorize(identity, parameter, logger, throwOnUnauthorized);
        }

        /// <summary>
        /// Returns value indicating whether an authorization method, that depends on
        /// a parameter (entity, criteria etc.) value, has been implemented for the parameter type specified.
        /// </summary>
        /// <typeparam name="TParam">a type of parameter that the authorization (might) depend on</typeparam>
        /// <returns>True, if an authorization method for the parameter type is implemented (exists);
        /// otherwise - false</returns>
        public abstract bool AuthorizationImplementedForParam<TParam>();


        /// <summary>
        /// Override this method to implement actual authorization for a use case without parameters
        /// (i.e. whether the user is authorized to invoke a use case at least for some parameters)
        /// </summary>
        /// <param name="identity">user identity (guaranteed not null and authenticated)</param>
        /// <param name="throwOnUnauthorized">whether to throw an application specific not authorized exception
        /// if the user is not authorized</param>
        /// <param name="logger">a logger to use for authorization warnings</param>
        /// <returns>true if the user is authorized; false otherwise</returns>
        protected abstract bool Authorize(ClaimsIdentity identity, ILogger logger, bool throwOnUnauthorized);

        /// <summary>
        /// Override this method to implement actual authorization for a use case with a parameter
        /// (i.e. whether the user is authorized to invoke a use case for a given parameter (command parameter, query criteria, save parameter etc.))
        /// </summary>
        /// <typeparam name="TParam">a type of use case parameter to use for authorization</typeparam>
        /// <param name="identity">user identity (guaranteed not null and authenticated)</param>
        /// <param name="parameter">a use case parameter to use for authorization</param>
        /// <param name="throwOnUnauthorized">whether to throw an application specific not authorized exception
        /// if the user is not authorized</param>
        /// <param name="logger">a logger to use for authorization warnings</param>
        /// <returns>true if the user is authorized; false otherwise</returns>
        protected abstract bool Authorize<TParam>(ClaimsIdentity identity, TParam parameter,
            ILogger logger, bool throwOnUnauthorized);

        /// <summary>
        /// Override this method to throw an application specific not authenticated exception.
        /// </summary>
        protected abstract void ThrowNotAuthenticatedException();


        /// <summary>
        /// Gets the localized (authorization) error message string, coming from evaluating the 
        /// errorMessageResourceType and errorMessageResourceName pair.
        /// </summary>
        protected string ErrorMessageString
        {
            get
            {
                this.SetupResourceAccessor();
                return this._errorMessageResourceAccessor();
            }
        }

        /// <summary>
        /// Override this method to 
        /// </summary>
        protected virtual Type UseCaseType 
            => null;


        /// <summary>
        /// Validates the configuration of this attribute and sets up the appropriate error string accessor.
        /// This method bypasses all verification once the ResourceAccessor has been set.
        /// </summary>
        /// <exception cref="InvalidOperationException"> is thrown if the current attribute is malformed.</exception>
        private void SetupResourceAccessor()
        {
            if (null != this._errorMessageResourceAccessor)
            {
                var property = this._errorMessageResourceType.GetProperty(this._errorMessageResourceName, 
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                if (null != property)
                {
                    MethodInfo propertyGetter = property.GetGetMethod(true /*nonPublic*/);
                    // We only support internal and public properties
                    if (null == propertyGetter || (!propertyGetter.IsAssembly && !propertyGetter.IsPublic))
                    {
                        // Set the property to null so the exception is thrown as if the property wasn't found
                        property = null;
                    }
                }

                if (property == null)
                {
                    throw new InvalidOperationException($"Resource type {_errorMessageResourceType.FullName} does not have a public or internal property {_errorMessageResourceName}.");
                }
                if (property.PropertyType != typeof(string))
                {
                    throw new InvalidOperationException($"Resource type {_errorMessageResourceType.FullName} property {_errorMessageResourceName} is not string.");
                }

                this._errorMessageResourceAccessor = delegate {
                    return (string)property.GetValue(null, null);
                };
            }
        }

        private void EnsureValidAuthorizationParameters(Type useCaseType, ClaimsIdentity identity, ILogger logger)
        {
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));
            if (null == useCaseType) throw new ArgumentNullException(nameof(useCaseType));
            if (null != useCaseType && null != UseCaseType && !UseCaseType.IsAssignableFrom(useCaseType) 
                && !useCaseType.IsAssignableFrom(UseCaseType))
                throw new InvalidOperationException(
                    $"Authorization attribute {this.GetType().FullName} can only be used on use case type {UseCaseType.FullName}.");
                  
            if (!identity.IsAuthenticated)
            {
                logger?.LogWarning($"Unauthenticated user attempted to invoke use case of type {useCaseType.FullName}.");
                ThrowNotAuthenticatedException();
            }
        }

    }
}
