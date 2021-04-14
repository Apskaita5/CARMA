using A5Soft.CARMA.Application.Authorization;
using System;
using System.Security.Claims;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base class for all use cases that implement authorization.
    /// </summary>
    /// <remarks>Authorization is only meaningful for remote execution.</remarks>
    public abstract class AuthorizedUseCaseBase : RemoteUseCaseBase, IAuthorizedUseCase
    {
        /// <summary>
        /// Gets an authorizer for the use case.
        /// </summary>
        protected readonly IUseCaseAuthorizer Authorizer;


        /// <inheritdoc />
        protected AuthorizedUseCaseBase(ClaimsIdentity user, IUseCaseAuthorizer authorizer, 
            IClientDataPortal dataPortal, IValidationEngineProvider validationProvider,
            IMetadataProvider metadataProvider, ILogger logger) :
            base(dataPortal, validationProvider, metadataProvider, logger)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Authorizer = authorizer ?? throw new ArgumentNullException(nameof(authorizer));
        }


        /// <inheritdoc cref="IAuthorizedUseCase.User" />
        public ClaimsIdentity User { get; }

        /// <inheritdoc cref="IAuthorizedUseCase.CanInvoke" />
        public bool CanInvoke(bool throwOnNotAuthorized = false)
        {
            return Authorizer.IsAuthorized(User, throwOnNotAuthorized);
        }
 
    }
}
