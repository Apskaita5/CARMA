using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a file upload operation without options.
    /// </summary>
    public abstract class UploadFileUseCaseBase : AuthorizedUseCaseBase
    {
        protected readonly ILogger _logger;
        private readonly IClientDataPortal _dataPortal;


        /// <inheritdoc />
        protected UploadFileUseCaseBase(ILogger logger, IClientDataPortal dataPortal,
            IAuthorizationProvider authorizationProvider, ClaimsIdentity userIdentity) 
            : base(authorizationProvider, userIdentity)
        {
            _dataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            _logger = logger;
        }


        /// <summary>
        /// Uploads a file.
        /// </summary>
        /// <param name="file">a file to upload</param>
        public async Task UploadFileAsync(FileContent file)
        {
            if (file.IsNull()) throw new ArgumentNullException(nameof(file));

            _logger?.LogMethodEntry(this.GetType(), nameof(UploadFileAsync));

            if (_dataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(file);
                    await _dataPortal.InvokeAsync(this.GetType().GetRemoteServiceInterfaceType(), file, User);
                    await AfterDataPortalAsync(file);
                }
                catch (Exception e)
                {
                    _logger?.LogError(e);
                    throw;
                }

                _logger?.LogMethodExit(this.GetType(), nameof(UploadFileAsync));

                return;
            }

            CanInvoke(true);

            try
            {
                await SaveAsync(file);
            }
            catch (Exception e)
            {
                _logger?.LogError(e);
                throw;
            }

            _logger?.LogMethodExit(this.GetType(), nameof(UploadFileAsync));
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
