﻿using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Rules;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for domain entity save operation (insert a new entity or update an existing one)
    /// with extra options (that affect save method behaviour, e.g. optionally send invoice to client).
    /// </summary>
    /// <typeparam name="TDomObject">a type of domain entity (must be binary serializable)</typeparam>
    /// <typeparam name="TDomInterface">a type of business interface</typeparam>
    /// <typeparam name="TOptions">a type of the save options (must be json serializable,
    /// i.e. either a primitive or a POCO)</typeparam>
    /// <remarks>Every entity is saved by passing its business only data by interface
    /// to make sure that inner state of the entity is not tampered in any way.</remarks>
    public abstract class SaveWithOptionsUseCaseBase<TDomObject, TDomInterface, TOptions> : AuthorizedUseCaseBase
        where TDomObject : class, ITrackState
        where TDomInterface : class, IDomainObject
    {
        protected readonly IValidationEngineProvider _validationEngineProvider;
        private readonly IClientDataPortal _dataPortal;
        protected readonly ILogger _logger;


        protected SaveWithOptionsUseCaseBase(IValidationEngineProvider validationEngineProvider, 
            ILogger logger, IClientDataPortal dataPortal, IAuthorizationProvider authorizationProvider, 
            ClaimsIdentity userIdentity) : base(authorizationProvider, userIdentity)
        {
            if (!typeof(TDomInterface).IsInterface) throw new InvalidOperationException(
                $"TDomInterface generic parameter for SaveUseCaseBase shall be an interface, while the provided parameter type is {typeof(TDomInterface).FullName}.");
            if (!typeof(TDomInterface).IsAssignableFrom(typeof(TDomObject))) throw new InvalidOperationException(
                $"Generic parameter type {typeof(TDomObject).FullName} does not implement interface defined by generic parameter TDomInterface - {typeof(TDomInterface).FullName}.");
            if (!typeof(TDomObject).IsSerializable) throw new InvalidOperationException(
                $"Domain entity must be (binary) serializable while type {typeof(TDomObject).FullName} is not.");
                                            
            _validationEngineProvider = validationEngineProvider ??
                throw new ArgumentNullException(nameof(validationEngineProvider));
            _dataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            _logger = logger;
        }


        /// <summary>
        /// Saves (persists) a domain entity data provided by interface,
        /// i.e. either creates a new domain entity or updates an existing entity.
        /// </summary>
        /// <param name="domainDto">business data for the domain entity to save</param>
        /// <param name="options">options that affect save method behaviour, e.g. optionally send invoice to client</param>
        /// <returns>a domain entity that has been saved (persisted)</returns>
        public async Task<TDomObject> SaveAsync(TDomInterface domainDto, TOptions options)
        {
            if (domainDto.IsNull()) throw new ArgumentNullException(nameof(domainDto));

            _logger?.LogMethodEntry(this.GetType(), nameof(SaveAsync), domainDto, options);

            TDomObject result;

            if (_dataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(domainDto, options);
                    result = await _dataPortal.FetchAsync<TDomInterface, TOptions, TDomObject>(
                        this.GetType().GetRemoteServiceInterfaceType(), domainDto, options, User);
                    if (result is ITrackState stateful) stateful.SetValidationEngine(_validationEngineProvider);
                    await AfterDataPortalAsync(domainDto, options, result);
                }
                catch (Exception e)
                {
                    _logger?.LogError(e);
                    throw;
                }

                _logger?.LogMethodExit(this.GetType(), nameof(SaveAsync), result);

                return result;
            }

            // Cannot trust user input (domainDto), no point to take it into account for authorization
            if (Authorizer.AuthorizationImplementedForParam<TOptions>())
                Authorizer.IsAuthorized(User, options, true);
            else CanInvoke(true);

            try
            {
                result = await DoSaveAsync(domainDto, options);
            }
            catch (Exception e)
            {
                _logger?.LogError(e);
                throw;
            }

            _logger?.LogMethodExit(this.GetType(), nameof(SaveAsync), result);

            return result;
        }

        /// <summary>
        /// Implement this method to load the business data to a new or existing domain entity,
        /// validate and save it (if valid).
        /// If required, implement any post save actions as well, e.g. resetting cache for relevant lookups,
        /// sending notifications etc.
        /// </summary>
        /// <param name="domainDto">business data for the domain entity to save</param> 
        /// <param name="options">options that affect save method behaviour, e.g. optionally send invoice to client</param>
        /// <returns>a domain entity that has been saved (persisted)</returns>
        /// <remarks>At this stage user has already been authorized
        /// and the domainDto param are guaranteed to be not null.
        /// The options param is NOT guaranteed to be not null (as it could be a valid value).
        /// However if the authorization depends on the entity data,
        /// appropriate authorization shall be applied in this method.
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<TDomObject> DoSaveAsync(TDomInterface domainDto, TOptions options);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(TDomInterface domainDto, TOptions options)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(TDomInterface domainDto, TOptions options, TDomObject result)
            => Task.CompletedTask;

    }
}
