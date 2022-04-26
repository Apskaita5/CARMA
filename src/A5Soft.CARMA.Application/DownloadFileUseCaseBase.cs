using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a file download.
    /// </summary>
    /// <typeparam name="T">a type of criteria for file (must be json serializable,
    /// i.e. either a primitive, an interface or a POCO)</typeparam>
    public abstract class DownloadFileUseCaseBase<T> : AuthorizedUseCaseBase
    {

        /// <inheritdoc />
        protected DownloadFileUseCaseBase(IAuthenticationStateProvider authenticationStateProvider, 
            IAuthorizationProvider authorizer, IClientDataPortal dataPortal, 
            IValidationEngineProvider validationProvider, IMetadataProvider metadataProvider, ILogger logger) 
            : base(authenticationStateProvider, authorizer, dataPortal, validationProvider, metadataProvider, logger)
        { }


        /// <summary>
        /// Downloads a file.
        /// </summary>
        /// <param name="criteria">a criteria for file</param>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a stream to download the file content</returns>
        public async Task<Stream> DownloadAsync(T criteria, CancellationToken ct = default)
        {
            if (null == criteria) throw new ArgumentNullException(nameof(criteria));

            Logger.LogMethodEntry(this.GetType(), nameof(DownloadAsync), criteria);
                            
            Stream result;

            if (DataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(criteria, ct);
                    result = await DataPortal.DownloadAsync<T>(this.GetType(),
                        criteria, await GetIdentityAsync(), ct);
                    await AfterDataPortalAsync(criteria, ct);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }

                Logger.LogMethodExit(this.GetType(), nameof(DownloadAsync), result);

                return result;
            }

            Authorizer.IsAuthorized(await GetIdentityAsync(), true);

            try
            {
                result = await DoDownloadAsync(criteria, ct);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }

            Logger.LogMethodExit(this.GetType(), nameof(DownloadAsync));

            return result;
        }


        /// <summary>
        /// Implement this method to download the file.
        /// </summary>
        /// <param name="criteria">a criteria for the file to download</param>
        /// <remarks>At this stage user has already been authorized
        /// and the criteria param is guaranteed to be not null.
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<Stream> DoDownloadAsync(T criteria, CancellationToken ct);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(T criteria, CancellationToken ct)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(T criteria, CancellationToken ct)
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
