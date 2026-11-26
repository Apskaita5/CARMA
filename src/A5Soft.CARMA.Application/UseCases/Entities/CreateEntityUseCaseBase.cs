using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Entities
{
    /// <summary>
    /// Creates (inserts) a new entity (optionally inited by <see cref="InitNewEntityUseCaseBase{TAppContext,TIdentity,TEntity}"/>).
    /// </summary>
    /// <remarks>Create and update are separate use cases: they usually need different permissions, and an
    /// <c>IsNew</c> coming from the client cannot be trusted.</remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TEntity">the type of the entity</typeparam>
    public abstract class CreateEntityUseCaseBase<TAppContext, TIdentity, TEntity> : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
        where TEntity : class, IDomainEntity<TEntity>
    {
        /// <inheritdoc />
        protected CreateEntityUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Creates the entity.
        /// </summary>
        /// <param name="entity">the new entity</param>
        /// <returns>the entity as persisted</returns>
        public Task<TEntity> CreateAsync(TEntity entity)
        {
            if (null == entity) throw new ArgumentNullException(nameof(entity));

            return CreateCoreAsync(entity);
        }

        /// <summary>
        /// Drops what the change has made stale from the scope's client cache, after it has succeeded: by default the
        /// lookups of the entity. Override to drop more, e.g. a cached query result
        /// (<c>Context.ClientCache.Invalidate(typeof(TResult))</c>).
        /// </summary>
        protected virtual void InvalidateClientCache() => Context.ClientCache.InvalidateEntity(typeof(TEntity));


        private async Task<TEntity> CreateCoreAsync(TEntity entity)
        {
            var result = await RunAsync(dp => dp.FetchAsync<TEntity, TEntity>(GetType(), entity),
                () => DoCreateAsync(entity), entity, nameof(CreateAsync))
                .ConfigureAwait(false);

            InvalidateClientCache();
            return result;
        }


        /// <summary>
        /// Gets the metadata of the entity, for the form. Local.
        /// </summary>
        public IEntityMetadata GetMetadata() => GetMetadataFor<TEntity>();


        /// <summary>
        /// Validates the entity, which comes from the client (CheckRules; the app's ValidationException), persists
        /// it as new and returns it as persisted.
        /// </summary>
        /// <param name="untrusted">the entity as received</param>
        protected abstract Task<TEntity> DoCreateAsync(TEntity untrusted);
    }
}
