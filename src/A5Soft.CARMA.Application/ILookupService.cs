using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Fetches lookup lists on behalf of a use case: the only way a lookup is fetched. Implemented by the app,
    /// deriving from <see cref="GenericLookupService"/> and marked
    /// <c>[DefaultServiceImplementation(typeof(ILookupService))]</c>.
    /// </summary>
    /// <remarks>
    /// <para>A user never calls it directly: use cases call it through
    /// <see cref="AuthorizedUseCaseBase{TAppContext,TIdentity}.FetchLookupAsync{TLookup,TEntity}"/>, which passes
    /// their own type as the requester. A fetch is allowed if the requester is a use case that lists the lookup
    /// in its <see cref="UseCaseAttribute.LookupTypes"/> and the scope's identity may invoke it, so the interface
    /// itself allows anonymous calls.</para>
    /// <para>The list returned may be cached and shared: never change it.</para>
    /// </remarks>
    [RemoteService(ServiceLifetime.Scoped)]
    public interface ILookupService
    {
        /// <summary>
        /// Fetches a lookup list on behalf of a use case.
        /// </summary>
        /// <typeparam name="TLookup">the type of the lookup</typeparam>
        /// <typeparam name="TEntity">the type of the entity the lookup refers to</typeparam>
        /// <param name="requesterType">the type of the use case that requests the lookup</param>
        /// <param name="cancellationToken">a token to cancel the fetch</param>
        /// <returns>the lookup list; it may be cached and shared, so never change it</returns>
        [RemoteMethod]
        Task<List<TLookup>> FetchAsync<TLookup, TEntity>(Type requesterType, CancellationToken cancellationToken = default)
            where TLookup : LookupBase<TEntity>
            where TEntity : class, IDomainEntity<TEntity>;
    }
}
