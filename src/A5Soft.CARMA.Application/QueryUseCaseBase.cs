using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a query operation without a criteria
    /// (that gets a query result that is not dependent on any criteria).
    /// </summary>
    /// <typeparam name="TResult">a type of the query result (must be binary serializable)</typeparam>
    public abstract class QueryUseCaseBase<TResult> : AuthorizedUseCaseBase
        where TResult : class
    {
        protected readonly ILogger _logger;
        private readonly IClientDataPortal _dataPortal;


        /// <inheritdoc />
        protected QueryUseCaseBase(ILogger logger, IClientDataPortal dataPortal,
            IAuthorizationProvider authorizationProvider, ClaimsIdentity userIdentity) 
            : base(authorizationProvider, userIdentity)
        {
            if (!typeof(TResult).IsSerializable) throw new InvalidOperationException(
                $"Query result must be (binary) serializable while type {typeof(TResult).FullName} is not.");

            _dataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            _logger = logger;
        }


        /// <summary>
        /// Gets a query result (that does not depend on any criteria).
        /// </summary>
        /// <returns>a query result</returns>
        public async Task<TResult> InvokeAsync()
        {
            _logger?.LogMethodEntry(this.GetType(), nameof(InvokeAsync));

            TResult result;

            if (_dataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync();
                    result = await _dataPortal.InvokeAsync<TResult>(
                        this.GetType().GetRemoteServiceInterfaceType(), User);
                    await AfterDataPortalAsync(result);
                }
                catch (Exception e)
                {
                    _logger?.LogError(e);
                    throw;
                }

                _logger?.LogMethodExit(this.GetType(), nameof(InvokeAsync), result);

                return result;
            }

            CanInvoke(true);

            try
            {
                result = await QueryAsync();
            }
            catch (Exception e)
            {
                _logger?.LogError(e);
                throw;
            }

            _logger?.LogMethodExit(this.GetType(), nameof(InvokeAsync), result);

            return result;
        }

        /// <summary>
        /// Implement this method to fetch a query result.
        /// </summary>
        /// <returns>a query result</returns>
        /// <remarks>At this stage user has already been authorized.
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<TResult> QueryAsync();

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
        protected virtual Task AfterDataPortalAsync(TResult result)
            => Task.CompletedTask;

    }
}
