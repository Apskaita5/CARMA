using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Rules;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for domain entity save operation (insert a new entity or update an existing one).
    /// </summary>
    /// <typeparam name="TDomObject">a type of domain entity (must be binary serializable)</typeparam>
    /// <typeparam name="TDomInterface">a type of business interface</typeparam>
    /// <remarks>Every entity is saved by passing its business only data by interface
    /// to make sure that inner state of the entity is not tampered in any way.</remarks>
    public abstract class SaveUseCaseBase<TDomObject, TDomInterface> : AuthorizedUseCaseBase
        where TDomObject : class, ITrackState
        where TDomInterface : class, IDomainObject
    {
        /// <inheritdoc />
        protected SaveUseCaseBase(ClaimsIdentity user, IUseCaseAuthorizer authorizer, 
            IClientDataPortal dataPortal, IValidationEngineProvider validationProvider, 
            IMetadataProvider metadataProvider, ILogger logger) 
            : base(user, authorizer, dataPortal, validationProvider, metadataProvider, logger)
        {
            if (!typeof(TDomInterface).IsInterface) throw new InvalidOperationException(
                $"TDomInterface generic parameter for SaveUseCaseBase shall be an interface, while the provided parameter type is {typeof(TDomInterface).FullName}.");
            if (!typeof(TDomInterface).IsAssignableFrom(typeof(TDomObject))) throw new InvalidOperationException(
                $"Generic parameter type {typeof(TDomObject).FullName} does not implement interface defined by generic parameter TDomInterface - {typeof(TDomInterface).FullName}.");
            if (!typeof(TDomObject).IsSerializable) throw new InvalidOperationException(
                $"Domain entity must be (binary) serializable while type {typeof(TDomObject).FullName} is not.");
        }


        /// <summary>
        /// Saves (persists) a domain entity data provided by interface,
        /// i.e. either creates a new domain entity or updates an existing entity.
        /// </summary>
        /// <param name="domainDto">business data for the domain entity to save</param>
        /// <returns>a domain entity that has been saved (persisted)</returns>
        public async Task<TDomObject> SaveAsync(TDomInterface domainDto)
        {
            if (domainDto.IsNull()) throw new ArgumentNullException(nameof(domainDto));

            Logger.LogMethodEntry(this.GetType(), nameof(SaveAsync), domainDto);

            TDomObject result;

            if (DataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(domainDto);
                    result = await DataPortal.FetchAsync<TDomInterface, TDomObject>(
                        this.GetType(), domainDto, User);
                    if (result is ITrackState stateful) stateful.SetValidationEngine(ValidationProvider);
                    await AfterDataPortalAsync(domainDto, result);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }

                Logger.LogMethodExit(this.GetType(), nameof(SaveAsync), result);

                return result;
            }

            // Cannot trust user input (domainDto), no point to take it into account for authorization
            CanInvoke(true);

            try
            {
                result = await DoSaveAsync(domainDto);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }

            Logger.LogMethodExit(this.GetType(), nameof(SaveAsync), result);

            return result;
        }

        /// <summary>
        /// Implement this method to load the business data to a new or existing domain entity,
        /// validate and save it (if valid).
        /// If required, implement any post save actions as well, e.g. resetting cache for relevant lookups,
        /// sending notifications etc.
        /// </summary>
        /// <param name="domainDto">business data for the domain entity to save</param>
        /// <returns>a domain entity that has been saved (persisted)</returns>
        /// <remarks>At this stage user has already been authorized
        /// and the domainDto param is guaranteed to be not null.
        /// However if the authorization depends on the entity data,
        /// appropriate authorization shall be applied in this method.
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<TDomObject> DoSaveAsync(TDomInterface domainDto);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(TDomInterface domainDto)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(TDomInterface domainDto, TDomObject result)
            => Task.CompletedTask;

    }
}
