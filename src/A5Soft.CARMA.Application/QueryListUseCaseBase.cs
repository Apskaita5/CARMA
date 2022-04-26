using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a query operation without a criteria when the result is a list of query results.
    /// </summary>
    /// <typeparam name="TResult">a type of the query result (list item; must be binary serializable)</typeparam>
    public abstract class QueryListUseCaseBase<TResult> : AuthorizedUseCaseBase
        where TResult : class
    {

        /// <inheritdoc />
        protected QueryListUseCaseBase(IAuthenticationStateProvider authenticationStateProvider, 
            IAuthorizationProvider authorizer, IClientDataPortal dataPortal, 
            IValidationEngineProvider validationProvider, IMetadataProvider metadataProvider, ILogger logger)
            : base(authenticationStateProvider, authorizer, dataPortal, validationProvider, metadataProvider, logger)
        {
            if (!typeof(TResult).IsSerializable) throw new InvalidOperationException(
                $"Query result must be (binary) serializable while type {typeof(TResult).FullName} is not.");
        }


        /// <summary>
        /// Gets a query result (that does not depend on any criteria).
        /// </summary>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a query result</returns>
        public async Task<List<TResult>> QueryAsync(CancellationToken ct = default)
        {
            Logger.LogMethodEntry(this.GetType(), nameof(QueryAsync));

            List<TResult> result;

            if (DataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(ct);
                    result = (await DataPortal.FetchAsync<List<TResult>>(this.GetType(), await GetIdentityAsync(), ct)).Result;
                    await AfterDataPortalAsync(result, ct);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }

                Logger.LogMethodExit(this.GetType(), nameof(QueryAsync), result);

                return result;
            }

            await CanInvokeAsync(true);

            try
            {
                result = await DoQueryAsync(ct);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }

            Logger.LogMethodExit(this.GetType(), nameof(QueryAsync), result);

            return result;
        }

        /// <summary>
        /// Implement this method to fetch a query result.
        /// </summary>
        /// <returns>a query result</returns>
        /// <remarks>At this stage user has already been authorized.
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<List<TResult>> DoQueryAsync(CancellationToken ct);

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
        protected virtual Task AfterDataPortalAsync(List<TResult> result, CancellationToken ct)
            => Task.CompletedTask;

        /// <summary>
        /// Gets metadata for the entity fetched.
        /// </summary>
        public IEntityMetadata GetMetadata()
            => MetadataProvider.GetEntityMetadata<TResult>();

        /// <inheritdoc cref="IUseCaseMetadata.GetMenuTitle"/>
        public string GetMenuTitle()
        {
            return MetadataProvider.GetUseCaseMetadata(this.GetType()).GetMenuTitle();
        }
    }
}
