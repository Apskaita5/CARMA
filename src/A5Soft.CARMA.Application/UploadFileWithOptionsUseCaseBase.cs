using System;
using System.Security.Claims;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a file upload operation with options.
    /// </summary>
    /// <typeparam name="TOptions">a type of options (must be json serializable,
    /// i.e. either a primitive or a POCO)</typeparam>
    public abstract class UploadFileWithOptionsUseCaseBase<TOptions> : AuthorizedUseCaseBase
    {
        protected readonly ILogger _logger;
        private readonly IClientDataPortal _dataPortal;


        /// <inheritdoc />
        protected UploadFileWithOptionsUseCaseBase(ILogger logger, IClientDataPortal dataPortal,
            IAuthorizationProvider authorizationProvider, ClaimsIdentity userIdentity)
            : base(authorizationProvider, userIdentity)
        {
            _dataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            _logger = logger;
        }


        /// <summary>
        /// Uploads a file using the options specified.
        /// </summary>
        /// <param name="options">options that affect file save behaviour,
        /// e.g. to associate the file with some domain entity</param>
        /// <param name="file">a file to upload</param>
        public async Task InvokeAsync(FileContent file, TOptions options)
        {
            if (file.IsNull()) throw new ArgumentNullException(nameof(file));

            _logger?.LogMethodEntry(this.GetType(), nameof(InvokeAsync), options);

            if (_dataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(file, options);
                    await _dataPortal.InvokeAsync(this.GetType().GetRemoteServiceInterfaceType(), file, options, User);
                    await AfterDataPortalAsync(file, options);
                }
                catch (Exception e)
                {
                    _logger?.LogError(e);
                    throw;
                }

                _logger?.LogMethodExit(this.GetType(), nameof(InvokeAsync));

                return;
            }

            if (Authorizer.AuthorizationImplementedForParam<TOptions>())
                Authorizer.IsAuthorized(User, options, true);
            else CanInvoke(true);

            try
            {
                await SaveAsync(file, options);
            }
            catch (Exception e)
            {
                _logger?.LogError(e);
                throw;
            }

            _logger?.LogMethodExit(this.GetType(), nameof(InvokeAsync));
        }

        /// <summary>
        /// Implement this method to save the uploaded file.
        /// If required, implement any post save actions as well, e.g. resetting cache for relevant lookups,
        /// sending notifications etc.
        /// </summary>
        /// <param name="file">a file to save</param>
        /// <param name="options">options that affect file save behaviour,
        /// e.g. to associate the file with some domain entity</param>
        /// <remarks>At this stage user has already been authorized
        /// and the file param is guaranteed to be not null.
        /// The options param is NOT guaranteed to be not null (as it could be a valid option).
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task SaveAsync(FileContent file, TOptions options);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(FileContent file, TOptions options)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(FileContent file, TOptions options)
            => Task.CompletedTask;

    }
}
