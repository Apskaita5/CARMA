using A5Soft.CARMA.Application.Authorization;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
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
        private readonly IAuthenticationStateProvider _authenticationStateProvider;

        /// <summary>
        /// Gets an authorizer for the use case.
        /// </summary>
        protected readonly IUseCaseAuthorizer Authorizer;


        /// <inheritdoc />
        protected AuthorizedUseCaseBase(IAuthenticationStateProvider authenticationStateProvider, 
            IAuthorizationProvider authorizer, IClientDataPortal dataPortal, 
            IValidationEngineProvider validationProvider, IMetadataProvider metadataProvider, ILogger logger) 
            : base(dataPortal, validationProvider, metadataProvider, logger)
        {
            _authenticationStateProvider = authenticationStateProvider 
                ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
            Authorizer = authorizer?.GetAuthorizer(this.GetType()) ?? throw new ArgumentNullException(nameof(authorizer));
        }


        /// <inheritdoc cref="IAuthorizedUseCase.GetIdentityAsync" />
        public Task<ClaimsIdentity> GetIdentityAsync()
        {
            return _authenticationStateProvider.GetIdentityAsync();
        }

        /// <summary>
        /// Updates a current identity of the user.
        /// </summary>
        /// <param name="updatedIdentity">an updated identity of the user</param>
        protected async Task UpdateIdentityAsync(ClaimsIdentity updatedIdentity)
        {
            if (null == updatedIdentity) throw new ArgumentNullException(nameof(updatedIdentity));

            await _authenticationStateProvider.NotifyIdentityChangedAsync(updatedIdentity);
        }

        /// <inheritdoc cref="IAuthorizedUseCase.CanInvokeAsync" />
        public async Task<bool> CanInvokeAsync(bool throwOnNotAuthorized = false)
        {
            var currentIdentity = await _authenticationStateProvider.GetIdentityAsync();
            return Authorizer.IsAuthorized(currentIdentity, throwOnNotAuthorized);
        }
 
    }
}
