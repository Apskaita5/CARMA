using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Entities
{
    /// <summary>
    /// Updates an entity: fetches the persisted one, merges the one received into it and persists the result. The
    /// graph received is never persisted.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TEntity">the type of the entity</typeparam>
    public abstract class UpdateEntityUseCaseBase<TAppContext, TIdentity, TEntity> : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
        where TEntity : class, IDomainEntity<TEntity>
    {
        /// <inheritdoc />
        protected UpdateEntityUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Updates the entity.
        /// </summary>
        /// <param name="entity">the entity as edited</param>
        /// <returns>the entity as persisted</returns>
        public Task<TEntity> UpdateAsync(TEntity entity)
        {
            if (null == entity) throw new ArgumentNullException(nameof(entity));

            return UpdateCoreAsync(entity);
        }

        /// <summary>
        /// Drops what the change has made stale from the scope's client cache, after it has succeeded: by default the
        /// lookups of the entity. Override to drop more, e.g. a cached query result
        /// (<c>Context.ClientCache.Invalidate(typeof(TResult))</c>).
        /// </summary>
        protected virtual void InvalidateClientCache() => Context.ClientCache.InvalidateEntity(typeof(TEntity));

        private async Task<TEntity> UpdateCoreAsync(TEntity entity)
        {
            var result = await RunAsync(dp => dp.FetchAsync<TEntity, TEntity>(GetType(), entity),
                () => UpdateLocallyAsync(entity), entity, nameof(UpdateAsync)).ConfigureAwait(false);

            InvalidateClientCache();

            return result;
        }

        /// <summary>
        /// Gets the metadata of the entity, for the form. Local.
        /// </summary>
        public IEntityMetadata GetMetadata() => GetMetadataFor<TEntity>();


        /// <summary>
        /// Fetches the persisted entity; throws the app's NotFoundException if there is none.
        /// </summary>
        /// <param name="id">the id of the entity</param>
        protected abstract Task<TEntity> FetchPersistedAsync(DomainEntityIdentity<TEntity> id);

        /// <summary>
        /// Merges the entity received into the persisted one: <c>persisted.Merge(untrusted, true)</c> (concurrency
        /// check, one CheckRules pass, the app's ValidationException).
        /// </summary>
        /// <param name="persisted">the persisted entity</param>
        /// <param name="untrusted">the entity as received</param>
        protected abstract void Merge(TEntity persisted, TEntity untrusted);

        /// <summary>
        /// Persists the merged entity and returns it as persisted.
        /// </summary>
        /// <param name="merged">the persisted entity with the changes merged in</param>
        protected abstract Task<TEntity> DoUpdateAsync(TEntity merged);


        private async Task<TEntity> UpdateLocallyAsync(TEntity untrusted)
        {
            if (null == untrusted.Id) throw new ArgumentException(
                "An entity without an id cannot be updated.", nameof(untrusted));

            var persisted = await FetchPersistedAsync(untrusted.Id).ConfigureAwait(false);
            Merge(persisted, untrusted);
            return await DoUpdateAsync(persisted).ConfigureAwait(false);
        }
    }
}
