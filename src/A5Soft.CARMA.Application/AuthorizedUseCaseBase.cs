using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Domain;
using System;
using System.Security.Claims;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base class for all use cases that implement authorization.
    /// </summary>
    public abstract class AuthorizedUseCaseBase : IAuthorizedUseCase
    {

        /// <inheritdoc cref="IAuthorizedUseCase.User" />
        public ClaimsIdentity User { get; }

        /// <summary>
        /// Gets an authorizer for the use case.
        /// </summary>
        protected IUseCaseAuthorizer Authorizer { get; }


        protected AuthorizedUseCaseBase(IAuthorizationProvider authorizationProvider, ClaimsIdentity userIdentity)
        {
            if (authorizationProvider.IsNull()) throw new ArgumentNullException(nameof(authorizationProvider));

            User = userIdentity ?? throw new ArgumentNullException(nameof(userIdentity));
            Authorizer = authorizationProvider.GetAuthorizer(this.GetType());
        }


        /// <inheritdoc cref="IAuthorizedUseCase.CanInvoke" />
        public bool CanInvoke(bool throwOnNotAuthorized = false)
        {
            return Authorizer.IsAuthorized(User, throwOnNotAuthorized);
        }
 
    }
}
