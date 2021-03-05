using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Rules;
using System;
using System.Security.Claims;
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
        protected readonly IValidationEngineProvider _validationEngineProvider;
        private readonly IClientDataPortal _dataPortal;
        protected readonly ILogger _logger;


        protected FetchDomainSingletonUseCaseBase(IValidationEngineProvider validationEngineProvider,
            ILogger logger, IClientDataPortal dataPortal, IAuthorizationProvider authorizationProvider, 
            ClaimsIdentity userIdentity) : base(authorizationProvider, userIdentity)
        {
            if (!typeof(T).IsSerializable) throw new InvalidOperationException(
                $"Domain entity must be (binary) serializable while type {typeof(T).FullName} is not.");

            _validationEngineProvider = validationEngineProvider ??
                throw new ArgumentNullException(nameof(validationEngineProvider));
            _dataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            _logger = logger;
        }


        /// <summary>
        /// Fetches a domain singleton, i.e. a domain entity that only has one instance per domain,
        /// e.g. company settings. 
        /// </summary>
        /// <returns>a domain singleton</returns>
        public async Task<T> InvokeAsync()
        {
            _logger?.LogMethodEntry(this.GetType(), nameof(InvokeAsync));

            T result;

            if (_dataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync();
                    result = await _dataPortal.InvokeAsync<T>(
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
                result = await FetchAsync();
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
        /// Implement this method to fetch a domain singleton instance.
        /// </summary>
        /// <returns>a domain singleton instance</returns>
        /// <remarks>At this stage user has already been authorized.
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<T> FetchAsync();

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
        protected virtual Task AfterDataPortalAsync(T result)
            => Task.CompletedTask;

    }
}
