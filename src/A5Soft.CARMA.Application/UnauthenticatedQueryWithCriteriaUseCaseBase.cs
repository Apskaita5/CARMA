using A5Soft.CARMA.Application.DataPortal;
using System;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for an unauthenticated query operation (that gets a query result
    /// for an unauthenticated user using query criteria provided).
    /// E.g. login use case returns ClaimsIdentity.
    /// </summary>
    /// <typeparam name="TResult">a type of the query result (must be binary serializable)</typeparam>
    /// <typeparam name="TCriteria">a type of the query parameter (must be json serializable,
    /// i.e. either a primitive or a POCO)</typeparam>
    public abstract class UnauthenticatedQueryWithCriteriaUseCaseBase<TResult, TCriteria>
    {
        protected readonly ILogger _logger;
        private readonly IClientDataPortal _dataPortal;
            
        
        protected UnauthenticatedQueryWithCriteriaUseCaseBase(ILogger logger, IClientDataPortal dataPortal)
        {
            if (!typeof(TResult).IsSerializable) throw new InvalidOperationException(
                $"Query result must be (binary) serializable while type {typeof(TResult).FullName} is not.");

            _dataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            _logger = logger;
        }


        /// <summary>
        /// Gets a query result using query criteria provided.
        /// </summary>
        /// <param name="criteria">a criteria for the query</param>
        /// <returns>a query result</returns>
        public async Task<TResult> InvokeAsync(TCriteria criteria)
        {
            _logger?.LogMethodEntry(this.GetType(), nameof(InvokeAsync), criteria);

            TResult result;

            if (_dataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(criteria);
                    result = await _dataPortal.InvokeAsync<TCriteria, TResult>(
                        this.GetType().GetRemoteServiceInterfaceType(), criteria);
                    await AfterDataPortalAsync(criteria, result);
                }
                catch (Exception e)
                {
                    _logger?.LogError(e);
                    throw;
                }

                _logger?.LogMethodExit(this.GetType(), nameof(InvokeAsync));

                return result;
            }

            try
            {
                result = await QueryIntAsync(criteria);
            }
            catch (Exception e)
            {
                _logger?.LogError(e);
                throw;
            }

            _logger?.LogMethodExit(this.GetType(), nameof(InvokeAsync));

            return result;
        }


        /// <summary>
        /// Implement this method to fetch a query result.
        /// </summary>
        /// <param name="criteria">a criteria for the query</param>
        /// <returns>a query result</returns>
        /// <remarks>The criteria param is NOT guaranteed to be not null (as it could be a valid option).
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<TResult> QueryIntAsync(TCriteria criteria);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(TCriteria criteria)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. 
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(TCriteria criteria, TResult result)
            => Task.CompletedTask;

    }
}
