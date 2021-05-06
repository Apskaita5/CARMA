using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a file upload operation without options.
    /// </summary>
    public abstract class UploadFileUseCaseBase : AuthorizedUseCaseBase
    {
        /// <inheritdoc />
        protected UploadFileUseCaseBase(IAuthenticationStateProvider authenticationStateProvider, 
            IAuthorizationProvider authorizer, IClientDataPortal dataPortal, 
            IValidationEngineProvider validationProvider, IMetadataProvider metadataProvider, ILogger logger) 
            : base(authenticationStateProvider, authorizer, dataPortal, validationProvider, metadataProvider, logger) { }


        /// <summary>
        /// Uploads a file.
        /// </summary>
        /// <param name="file">a file to upload</param>
        public async Task UploadFileAsync(FileContent file)
        {
            if (file.IsNull()) throw new ArgumentNullException(nameof(file));

            Logger.LogMethodEntry(this.GetType(), nameof(UploadFileAsync));

            if (DataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(file);
                    await DataPortal.InvokeAsync(this.GetType(), file, await GetIdentityAsync());
                    await AfterDataPortalAsync(file);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }

                Logger.LogMethodExit(this.GetType(), nameof(UploadFileAsync));

                return;
            }

            await CanInvokeAsync(true);

            try
            {
                await SaveAsync(file);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }

            Logger.LogMethodExit(this.GetType(), nameof(UploadFileAsync));
        }

        /// <summary>
        /// Implement this method to save the uploaded file.
        /// If required, implement any post save actions as well, e.g. resetting cache for relevant lookups,
        /// sending notifications etc.
        /// </summary>
        /// <param name="file">a file to save</param>
        /// <remarks>At this stage user has already been authorized
        /// and the file param is guaranteed to be not null.
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task SaveAsync(FileContent file);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(FileContent file)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(FileContent file)
            => Task.CompletedTask;

    }
}
