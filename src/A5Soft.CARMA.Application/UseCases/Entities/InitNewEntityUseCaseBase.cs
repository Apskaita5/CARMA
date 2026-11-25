using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Entities
{
    /// <summary>
    /// Inits a new, unsaved entity with its defaults applied; <see cref="CreateEntityUseCaseBase{TAppContext,TIdentity,TEntity}"/>
    /// persists it.
    /// </summary>
    /// <remarks>Runs on the server like anything else, as the defaults may need server side data (settings, the
    /// next document number). It cannot be cancelled: an app may reserve something (a number) while
    /// initing.</remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TEntity">the type of the entity</typeparam>
    public abstract class InitNewEntityUseCaseBase<TAppContext, TIdentity, TEntity> : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
        where TEntity : class, IDomainEntity<TEntity>
    {
        /// <inheritdoc />
        protected InitNewEntityUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Inits a new entity.
        /// </summary>
        /// <returns>a new, unsaved entity</returns>
        public Task<TEntity> InitNewAsync() => RunAsync(dp => dp.FetchAsync<TEntity>(GetType()), DoInitNewAsync);

        /// <summary>
        /// Gets the metadata of the entity, for the form. Local.
        /// </summary>
        public IEntityMetadata GetMetadata() => GetMetadataFor<TEntity>();


        /// <summary>
        /// Inits the new entity.
        /// </summary>
        protected abstract Task<TEntity> DoInitNewAsync();
    }
}
