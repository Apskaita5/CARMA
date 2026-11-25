using A5Soft.CARMA.Domain.Metadata;
using System;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base class for all use cases: the scope's application context.
    /// </summary>
    /// <remarks>A use case that never runs remotely derives from it directly; every other one derives from
    /// <see cref="AuthorizedUseCaseBase{TAppContext,TIdentity}"/> or one of the bases built on it.</remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    public abstract class UseCaseBase<TAppContext> : IUseCase
        where TAppContext : class, IApplicationContext
    {
        /// <summary>
        /// Application context and common services.
        /// </summary>
        protected readonly TAppContext Context;


        protected UseCaseBase(TAppContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }


        /// <summary>
        /// Gets the metadata of a domain type, for a form or a grid. Local: never crosses the data portal.
        /// </summary>
        /// <typeparam name="T">the domain type</typeparam>
        protected IEntityMetadata GetMetadataFor<T>() => Context.MetadataProvider.GetEntityMetadata(typeof(T));
    }
}
