using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Rules;
using System;
using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a domain entity fetch operation (that gets a domain entity by its identity).
    /// </summary>
    /// <typeparam name="T">a type of domain entity to fetch (must be binary serializable)</typeparam>
    public abstract class FetchDomainEntityUseCaseBase<T> : AuthorizedUseCaseBase
        where T : class, IDomainEntity
    {
        /// <inheritdoc />
        protected FetchDomainEntityUseCaseBase(IAuthenticationStateProvider authenticationStateProvider, 
            IAuthorizationProvider authorizer, IClientDataPortal dataPortal, 
            IValidationEngineProvider validationProvider, IMetadataProvider metadataProvider, ILogger logger) 
            : base(authenticationStateProvider, authorizer, dataPortal, validationProvider, metadataProvider, logger)
        {
            if (!typeof(T).IsSerializable) throw new InvalidOperationException(
                $"Fetch result must be (binary) serializable while type {typeof(T).FullName} is not.");
        }


        /// <summary>
        /// Gets a domain entity by its identity.
        /// </summary>
        /// <param name="id">an identity of the entity to fetch</param>
        /// <param name="ct">cancellation token (if any)</param>
        /// <returns>a domain entity</returns>
        public async Task<T> FetchAsync(IDomainEntityIdentity id, CancellationToken ct = default)
        {
            if (id.IsNull()) throw new ArgumentNullException(nameof(id));

            Logger.LogMethodEntry(this.GetType(), nameof(FetchAsync), id);

            if (id.IsNew) throw new ArgumentException(
                "Entity id must reference an already existing entity.", nameof(id));
            if (id.DomainEntityType != typeof(T)) throw new ArgumentException(
                $"Specified id references entity type {id.DomainEntityType.FullName} " +
                $"while the use case requires entity type {typeof(T).FullName}.");

            T result;

            if (DataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(id, ct);
                    result = await DataPortal.FetchAsync<IDomainEntityIdentity, T>(this.GetType(), 
                        id, await GetIdentityAsync(), ct);
                    if (result is ITrackState stateful) stateful.SetValidationEngine(ValidationProvider);
                    await AfterDataPortalAsync(id, result, ct);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }

                Logger.LogMethodExit(this.GetType(), nameof(FetchAsync), result);

                return result;
            }

            Authorizer.IsAuthorized(await GetIdentityAsync(), true);

            try
            {
                result = await DoFetchAsync(id, ct);
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
        /// Implement this method to fetch a domain entity instance.
        /// </summary>
        /// <param name="id">an identity of the entity to fetch</param>
        /// <returns>a domain entity</returns>
        /// <remarks>At this stage user has already been authorized
        /// and the id param is guaranteed to be not null.
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<T> DoFetchAsync(IDomainEntityIdentity id, CancellationToken ct);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(IDomainEntityIdentity id, CancellationToken ct)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(IDomainEntityIdentity id, T result, CancellationToken ct)
            => Task.CompletedTask;

        /// <summary>
        /// Gets metadata for the entity fetched.
        /// </summary>
        public IEntityMetadata GetMetadata() 
            => MetadataProvider.GetEntityMetadata<T>();

    }
}
