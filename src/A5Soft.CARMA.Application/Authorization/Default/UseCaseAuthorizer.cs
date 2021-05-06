using System;
using System.Linq;
using System.Security.Claims;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization.Default
{
    internal class UseCaseAuthorizer : IUseCaseAuthorizer
    {
        private readonly AuthorizationBaseAttribute _authorizationAttribute;
        private readonly ILogger _logger;


        /// <inheritdoc cref="IUseCaseAuthorizer.UseCaseType" />
        public Type UseCaseType { get; }


        public UseCaseAuthorizer(Type useCaseType, ILogger logger)
        {
            if (null == useCaseType) throw new ArgumentNullException(nameof(useCaseType));
            if (!useCaseType.IsClass && !useCaseType.IsInterface) throw new ArgumentException(
                $"Authorization can only be defined for classes or interfaces, while {useCaseType.FullName} is not.", 
                nameof(useCaseType));

            _logger = logger;
            UseCaseType = useCaseType;

            var authorizationAttributes = useCaseType.GetCustomAttributes(typeof(AuthorizationBaseAttribute), false);
            if ((null == authorizationAttributes || authorizationAttributes.Length < 1) && useCaseType.IsClass)
            {
                foreach (Type entityInterface in useCaseType.GetInterfaces())
                {
                    authorizationAttributes = entityInterface.GetCustomAttributes(typeof(AuthorizationBaseAttribute), false);
                    if (null != authorizationAttributes && authorizationAttributes.Length > 0) break;
                }
            }

            if (null == authorizationAttributes || authorizationAttributes.Length < 1)
                throw new InvalidOperationException(
                    $"Authorization attribute is not defined for entity (or interface) of type {useCaseType.FullName}.");

            _authorizationAttribute = (AuthorizationBaseAttribute)authorizationAttributes[0];
        }


        /// <inheritdoc cref="IUseCaseAuthorizer.IsAuthorized" />
        public bool IsAuthorized(ClaimsIdentity identity, bool throwOnUnauthorized = false)
        {
            return _authorizationAttribute.IsAuthorized(UseCaseType, identity, _logger, throwOnUnauthorized);
        }

        /// <inheritdoc cref="IUseCaseAuthorizer.IsAuthorized" />
        public bool IsAuthorized<TParam>(ClaimsIdentity identity, TParam parameter, 
            bool throwOnUnauthorized = false)
        {
            return _authorizationAttribute.IsAuthorized(UseCaseType, identity, parameter,
                _logger, throwOnUnauthorized);
        }

        /// <inheritdoc cref="IUseCaseAuthorizer.AuthorizationImplementedForParam{TParam}" />
        public bool AuthorizationImplementedForParam<TParam>()
        {
            return _authorizationAttribute.AuthorizationImplementedForParam<TParam>();
        }

    }
}
