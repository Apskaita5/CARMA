using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;
using System;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Entities
{
    /// <summary>
    /// Deletes an entity by its id.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TEntity">the type of the entity</typeparam>
    public abstract class DeleteEntityUseCaseBase<TAppContext, TIdentity, TEntity> : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
        where TEntity : class, IDomainEntity<TEntity>
    {
        /// <inheritdoc />
        protected DeleteEntityUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Deletes the entity.
        /// </summary>
        /// <param name="id">the id of the entity</param>
        public Task DeleteAsync(DomainEntityIdentity<TEntity> id)
        {
            if (null == id) throw new ArgumentNullException(nameof(id));

            return DeleteCoreAsync(id);
        }

        /// <summary>
        /// Drops what the change has made stale from the scope's client cache, after it has succeeded: by default the
        /// lookups of the entity. Override to drop more, e.g. a cached query result
        /// (<c>Context.ClientCache.Invalidate(typeof(TResult))</c>).
        /// </summary>
        protected virtual void InvalidateClientCache() => Context.ClientCache.InvalidateEntity(typeof(TEntity));

        private async Task DeleteCoreAsync(DomainEntityIdentity<TEntity> id)
        {
            await RunAsync(dp => dp.InvokeAsync(GetType(), id), () => DoDeleteAsync(id), id, nameof(DeleteAsync))
                .ConfigureAwait(false);

            InvalidateClientCache();
        }

        /// <summary>
        /// Deletes the entity; throws the app's business exception if it cannot be deleted (e.g. it is in use).
        /// </summary>
        /// <param name="id">the id of the entity</param>
        protected abstract Task DoDeleteAsync(DomainEntityIdentity<TEntity> id);
    }
}
