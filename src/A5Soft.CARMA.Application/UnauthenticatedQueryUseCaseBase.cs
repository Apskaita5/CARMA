using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for an unauthenticated query operation (that gets a query result
    /// for an unauthenticated user without a query criteria).
    /// E.g. create signup request use case returns a new sign up request instance
    /// preinitialized with the profile values.
    /// </summary>
    /// <typeparam name="TResult">a type of the query result (must be binary serializable)</typeparam>
    public abstract class UnauthenticatedQueryUseCaseBase<TResult> : RemoteUseCaseBase
    {
        protected UnauthenticatedQueryUseCaseBase(IClientDataPortal dataPortal,
            IValidationEngineProvider validationProvider, IMetadataProvider metadataProvider,
            ILogger logger) : base(dataPortal, validationProvider, metadataProvider, logger)
        {}


        /// <summary>
        /// Gets a query result.
        /// </summary>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a query result</returns>
        public async Task<TResult> InvokeAsync(CancellationToken ct = default)
        {
            Logger.LogMethodEntry(this.GetType(), nameof(InvokeAsync));

            TResult result;

            if (DataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(ct);

                    var dpResult = await DataPortal.FetchUnauthenticatedAsync<TResult>(
                        this.GetType(), ct);

                    result = dpResult.Result;
                    if (result is ITrackState stateful) stateful.SetValidationEngine(ValidationProvider);

                    await AfterDataPortalAsync(result, ct);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }

                Logger.LogMethodExit(this.GetType(), nameof(InvokeAsync));

                return result;
            }

            try
            {
                result = await QueryIntAsync(ct);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }

            Logger.LogMethodExit(this.GetType(), nameof(InvokeAsync));

            return result;
        }


        /// <summary>
        /// Implement this method to fetch a query result.
        /// </summary>
        /// <returns>a query result</returns>
        /// <remarks>This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<TResult> QueryIntAsync(CancellationToken ct);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used.
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(CancellationToken ct)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation.
        /// Only invoked if a remote data portal is used.
        /// </summary>
        protected virtual Task AfterDataPortalAsync(TResult result, CancellationToken ct)
            => Task.CompletedTask;


        /// <summary>
        /// Gets metadata for the entity fetched.
        /// Returns null if the entity is not a class or an interface.
        /// </summary>
        public IEntityMetadata GetMetadata()
        {
            var resultType = typeof(TResult);
            if (!resultType.IsInterface && !resultType.IsClass) return null;
            if (resultType == typeof(string)) return null;
            return MetadataProvider.GetEntityMetadata<TResult>();
        }
    }
}
