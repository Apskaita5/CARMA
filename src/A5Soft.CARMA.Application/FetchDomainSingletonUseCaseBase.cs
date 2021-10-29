using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a domain singleton fetch operation
    /// (that gets a domain entity that only has one instance per domain, e.g. company settings).
    /// </summary>
    /// <typeparam name="T">a type of the domain singleton to fetch (must be binary serializable)</typeparam>
    public abstract class FetchDomainSingletonUseCaseBase<T> : AuthorizedUseCaseBase
        where T : class, IDomainObject
    {
        /// <inheritdoc />
        protected FetchDomainSingletonUseCaseBase(IAuthenticationStateProvider authenticationStateProvider, 
            IAuthorizationProvider authorizer, IClientDataPortal dataPortal, 
            IValidationEngineProvider validationProvider, IMetadataProvider metadataProvider, ILogger logger) 
            : base(authenticationStateProvider, authorizer, dataPortal, validationProvider, metadataProvider, logger)
        {
            if (!typeof(T).IsSerializable) throw new InvalidOperationException(
                $"Domain entity must be (binary) serializable while type {typeof(T).FullName} is not.");
        }


        /// <summary>
        /// Fetches a domain singleton, i.e. a domain entity that only has one instance per domain,
        /// e.g. company settings. 
        /// </summary>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a domain singleton</returns>
        public async Task<T> FetchAsync(CancellationToken ct = default)
        {
            Logger.LogMethodEntry(this.GetType(), nameof(FetchAsync));

            T result;

            if (DataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(ct);
                    result = (await DataPortal.FetchAsync<T>(this.GetType(), await GetIdentityAsync(), ct)).Result;
                    if (result is ITrackState statefulResult) 
                        statefulResult.SetValidationEngine(ValidationProvider);
                    await AfterDataPortalAsync(result, ct);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }

                Logger.LogMethodExit(this.GetType(), nameof(FetchAsync), result);

                return result;
            }

            await CanInvokeAsync(true);

            try
            {
                result = await DoFetchAsync(ct);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }

            Logger.LogMethodExit(this.GetType(), nameof(FetchAsync), result);

            return result;
        }

        /// <summary>
        /// Implement this method to fetch a domain singleton instance.
        /// </summary>
        /// <returns>a domain singleton instance</returns>
        /// <remarks>At this stage user has already been authorized.
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<T> DoFetchAsync(CancellationToken ct);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(CancellationToken ct)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(T result, CancellationToken ct)
            => Task.CompletedTask;


        /// <summary>
        /// Gets metadata for the entity fetched.
        /// </summary>
        public IEntityMetadata GetMetadata()
            => MetadataProvider.GetEntityMetadata<T>();

    }
}
