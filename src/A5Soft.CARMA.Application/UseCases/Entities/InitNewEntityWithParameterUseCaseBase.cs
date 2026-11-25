using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Entities
{
    /// <summary>
    /// Inits a new, unsaved entity from a parameter: a copy of another entity, from a template, from another
    /// document. <see cref="CreateEntityUseCaseBase{TAppContext,TIdentity,TEntity}"/> persists it.
    /// </summary>
    /// <remarks>See <see cref="InitNewEntityUseCaseBase{TAppContext,TIdentity,TEntity}"/>.</remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TEntity">the type of the entity</typeparam>
    /// <typeparam name="TParameter">the type of the parameter</typeparam>
    public abstract class InitNewEntityWithParameterUseCaseBase<TAppContext, TIdentity, TEntity, TParameter>
        : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
        where TEntity : class, IDomainEntity<TEntity>
    {
        /// <inheritdoc />
        protected InitNewEntityWithParameterUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Inits a new entity from the parameter.
        /// </summary>
        /// <param name="parameter">the parameter, e.g. the id of the entity to copy</param>
        /// <returns>a new, unsaved entity</returns>
        public Task<TEntity> InitNewAsync(TParameter parameter)
        {
            if (null == parameter) throw new ArgumentNullException(nameof(parameter));

            return RunAsync(dp => dp.FetchAsync<TParameter, TEntity>(GetType(), parameter),
                () => DoInitNewAsync(parameter), parameter);
        }

        /// <summary>
        /// Gets the metadata of the entity, for the form. Local.
        /// </summary>
        public IEntityMetadata GetMetadata() => GetMetadataFor<TEntity>();


        /// <summary>
        /// Inits the new entity. The parameter comes from the client: do not trust it.
        /// </summary>
        /// <param name="parameter">the parameter</param>
        protected abstract Task<TEntity> DoInitNewAsync(TParameter parameter);
    }
}
