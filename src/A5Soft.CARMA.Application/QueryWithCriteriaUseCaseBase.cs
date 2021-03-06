using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a query operation (that gets a query result using query criteria provided).
    /// </summary>
    /// <typeparam name="TResult">a type of the query result (must be binary serializable)</typeparam>
    /// <typeparam name="TCriteria">a type of the query parameter (must be json serializable,
    /// i.e. either a primitive or a POCO)</typeparam>
    public abstract class QueryWithCriteriaUseCaseBase<TResult, TCriteria> : AuthorizedUseCaseBase
        where TResult : class
    {
        protected readonly ILogger _logger;
        private readonly IClientDataPortal _dataPortal;


        /// <inheritdoc />
        protected QueryWithCriteriaUseCaseBase(ILogger logger, IClientDataPortal dataPortal,
            IAuthorizationProvider authorizationProvider, ClaimsIdentity userIdentity) 
            : base(authorizationProvider, userIdentity)
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
        public async Task<TResult> QueryAsync(TCriteria criteria)
        {
            _logger?.LogMethodEntry(this.GetType(), nameof(QueryAsync), criteria);

            TResult result;

            if (_dataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(criteria);
                    result = await _dataPortal.FetchAsync<TCriteria, TResult>(
                        this.GetType().GetRemoteServiceInterfaceType(), criteria, User);
                    await AfterDataPortalAsync(criteria, result);
                }
                catch (Exception e)
                {
                    _logger?.LogError(e);
                    throw;
                }

                _logger?.LogMethodExit(this.GetType(), nameof(QueryAsync));

                return result;
            }

            if (Authorizer.AuthorizationImplementedForParam<TCriteria>())
                Authorizer.IsAuthorized(User, criteria, true);
            else CanInvoke(true);

            try
            {
                result = await DoQueryAsync(criteria);
            }
            catch (Exception e)
            {
                _logger?.LogError(e);
                throw;
            }

            _logger?.LogMethodExit(this.GetType(), nameof(QueryAsync));

            return result;
        }

        /// <summary>
        /// Implement this method to fetch a query result.
        /// </summary>
        /// <param name="criteria">a criteria for the query</param>
        /// <returns>a query result</returns>
        /// <remarks>At this stage user has already been authorized.
        /// The criteria param is NOT guaranteed to be not null (as it could be a valid option).
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<TResult> DoQueryAsync(TCriteria criteria);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(TCriteria criteria)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(TCriteria criteria, TResult result)
            => Task.CompletedTask;

    }
}
