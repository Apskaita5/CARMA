using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Entities
{
    /// <summary>
    /// Fetches an entity by its id. Does not change state, so it can be cancelled. Interface method:
    /// <c>Task&lt;TEntity&gt; FetchAsync(DomainEntityIdentity&lt;TEntity&gt; id,
    /// CancellationToken cancellationToken = default)</c>.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TEntity">the type of the entity</typeparam>
    public abstract class FetchEntityUseCaseBase<TAppContext, TIdentity, TEntity> : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
        where TEntity : class, IDomainEntity<TEntity>
    {
        /// <inheritdoc />
        protected FetchEntityUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Fetches the entity.
        /// </summary>
        /// <param name="id">the id of the entity</param>
        /// <param name="cancellationToken">a token to cancel the fetch</param>
        /// <returns>the entity</returns>
        public Task<TEntity> FetchAsync(DomainEntityIdentity<TEntity> id, CancellationToken cancellationToken = default)
        {
            if (null == id) throw new ArgumentNullException(nameof(id));

            return RunReadOnlyAsync(
                (dp, ct) => dp.FetchAsync<DomainEntityIdentity<TEntity>, TEntity>(GetType(), id, ct),
                async ct => await AfterFetchAsync(await DoFetchAsync(id, ct).ConfigureAwait(false), ct)
                    .ConfigureAwait(false),
                cancellationToken, id);
        }

        /// <summary>
        /// Gets the metadata of the entity, for the form. Local.
        /// </summary>
        public IEntityMetadata GetMetadata() => GetMetadataFor<TEntity>();


        /// <summary>
        /// Fetches the entity; throws the app's NotFoundException if there is none.
        /// </summary>
        /// <param name="id">the id of the entity</param>
        /// <param name="cancellationToken">a token to cancel the fetch</param>
        protected abstract Task<TEntity> DoFetchAsync(DomainEntityIdentity<TEntity> id, CancellationToken cancellationToken);

        /// <summary>
        /// Runs after the fetch, e.g. to hide the audit trace from a user who may not see it.
        /// </summary>
        /// <param name="entity">the entity fetched</param>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        /// <returns>the entity to return</returns>
        protected virtual Task<TEntity> AfterFetchAsync(TEntity entity, CancellationToken cancellationToken)
            => Task.FromResult(entity);
    }
}
