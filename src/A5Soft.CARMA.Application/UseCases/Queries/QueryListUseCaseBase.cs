using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Queries
{
    /// <summary>
    /// A query returning a list. Does not change state, so it can be cancelled.
    /// Interface method: <c>Task&lt;List&lt;TItem&gt;&gt; QueryAsync(CancellationToken cancellationToken = default)</c>.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TItem">the type of the items</typeparam>
    public abstract class QueryListUseCaseBase<TAppContext, TIdentity, TItem> : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
        where TItem : QueryResultItemBase
    {
        /// <inheritdoc />
        protected QueryListUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Runs the query.
        /// </summary>
        /// <param name="cancellationToken">a token to cancel the query</param>
        /// <returns>the result</returns>
        public Task<List<TItem>> QueryAsync(CancellationToken cancellationToken = default)
        {
            return RunReadOnlyAsync((dp, ct) => dp.FetchAsync<List<TItem>>(GetType(), ct),
                DoQueryAsync, cancellationToken);
        }

        /// <summary>
        /// Gets the metadata of the items, for the grid. Local.
        /// </summary>
        public IEntityMetadata GetResultMetadata() => GetMetadataFor<TItem>();


        /// <summary>
        /// Runs the query.
        /// </summary>
        /// <param name="cancellationToken">a token to cancel the query</param>
        protected abstract Task<List<TItem>> DoQueryAsync(CancellationToken cancellationToken);
    }
}
