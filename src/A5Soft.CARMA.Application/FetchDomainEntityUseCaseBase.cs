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
    /// A base use case for a domain entity fetch operation (that gets a domain entity by its identity).
    /// </summary>
    /// <typeparam name="T">a type of domain entity to fetch (must be binary serializable)</typeparam>
    public abstract class FetchDomainEntityUseCaseBase<T> : AuthorizedUseCaseBase
        where T : class, IDomainEntity
    {
        protected readonly IValidationEngineProvider _validationEngineProvider;
        private readonly IClientDataPortal _dataPortal;
        protected readonly ILogger _logger;


        /// <inheritdoc />
        protected FetchDomainEntityUseCaseBase(IValidationEngineProvider validationEngineProvider, 
            ILogger logger, IClientDataPortal dataPortal, IAuthorizationProvider authorizationProvider, 
            ClaimsIdentity userIdentity) : base(authorizationProvider, userIdentity)
        {
            if (!typeof(T).IsSerializable) throw new InvalidOperationException(
                $"Fetch result must be (binary) serializable while type {typeof(T).FullName} is not.");

            _validationEngineProvider = validationEngineProvider ??
                throw new ArgumentNullException(nameof(validationEngineProvider));
            _dataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            _logger = logger;
        }


        /// <summary>
        /// Gets a domain entity by its identity.
        /// </summary>
        /// <param name="id">an identity of the entity to fetch</param>
        /// <returns>a domain entity</returns>
        public async Task<T> InvokeAsync(IDomainEntityIdentity id)
        {
            if (id.IsNull()) throw new ArgumentNullException(nameof(id));

            _logger?.LogMethodEntry(this.GetType(), nameof(InvokeAsync), id);

            if (id.IsNew) throw new ArgumentException(
                "Entity id must reference an already existing entity.", nameof(id));

            T result;

            if (_dataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(id);
                    result = await _dataPortal.InvokeAsync<IDomainEntityIdentity, T>(
                        this.GetType().GetRemoteServiceInterfaceType(), id, User);
                    if (result is ITrackState stateful) stateful.SetValidationEngine(_validationEngineProvider);
                    await AfterDataPortalAsync(id, result);
                }
                catch (Exception e)
                {
                    _logger?.LogError(e);
                    throw;
                }

                _logger?.LogMethodExit(this.GetType(), nameof(InvokeAsync), result);

                return result;
            }

            if (Authorizer.AuthorizationImplementedForParam<IDomainEntityIdentity>())
                Authorizer.IsAuthorized(User, id, true);
            else Authorizer.IsAuthorized(User, true);

            try
            {
                result = await FetchAsync(id);
            }
            catch (Exception e)
            {
                _logger?.LogError(e);
                throw;
            }

            if (Authorizer.AuthorizationImplementedForParam<T>())
                Authorizer.IsAuthorized(User, result, true);

            _logger?.LogMethodExit(this.GetType(), nameof(InvokeAsync), result);

            return result;
        }

        /// <summary>
        /// Implement this method to fetch a domain entity instance.
        /// </summary>
        /// <param name="id">an identity of the entity to fetch</param>
        /// <returns>a domain entity</returns>
        /// <remarks>At this stage user has already been authorized
        /// and the id param is guaranteed to be not null.
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<T> FetchAsync(IDomainEntityIdentity id);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(IDomainEntityIdentity id)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(IDomainEntityIdentity id, T result)
            => Task.CompletedTask;

    }
}
