using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using System;
using System.Threading.Tasks;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a parameterless command operation (that executes a command without any parameters).
    /// </summary>
    public abstract class CommandUseCaseBase : AuthorizedUseCaseBase
    {
        /// <inheritdoc />
        protected CommandUseCaseBase(IAuthenticationStateProvider authenticationStateProvider, 
            IAuthorizationProvider authorizer, IClientDataPortal dataPortal, 
            IValidationEngineProvider validationProvider, IMetadataProvider metadataProvider, ILogger logger) 
            : base(authenticationStateProvider, authorizer, dataPortal, validationProvider, metadataProvider, logger) { }


        /// <summary>
        /// Executes the command.
        /// </summary>
        public async Task InvokeAsync()
        {
            Logger.LogMethodEntry(this.GetType(), nameof(InvokeAsync));

            if (DataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync();
                    await DataPortal.InvokeAsync(this.GetType(), await GetIdentityAsync());
                    await AfterDataPortalAsync();
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }
                
                Logger.LogMethodExit(this.GetType(), nameof(InvokeAsync));

                return;
            }

            await CanInvokeAsync(true);

            try
            {
                await ExecuteAsync();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }

            Logger.LogMethodExit(this.GetType(), nameof(InvokeAsync));
        }

        /// <summary>
        /// Implement this method to execute the command.
        /// </summary>
        /// <remarks>At this stage user has already been authorized.
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task ExecuteAsync();

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync()
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync()
            => Task.CompletedTask;

        /// <inheritdoc cref="IUseCaseMetadata.GetButtonTitle"/>
        public string GetButtonTitle()
        {
            return MetadataProvider.GetUseCaseMetadata(this.GetType()).GetButtonTitle();
        }

        /// <inheritdoc cref="IUseCaseMetadata.GetMenuTitle"/>
        public string GetMenuTitle()
        {
            return MetadataProvider.GetUseCaseMetadata(this.GetType()).GetMenuTitle();
        }
    }
}
