using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Queries
{
    /// <summary>
    /// A query with criteria returning a list. Does not change state, so it can be cancelled.
    /// Interface method: <c>Task&lt;List&lt;TItem&gt;&gt; QueryAsync(TCriteria criteria, CancellationToken cancellationToken = default)</c>.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TCriteria">the type of the criteria</typeparam>
    /// <typeparam name="TItem">the type of the items</typeparam>
    public abstract class QueryListWithCriteriaUseCaseBase<TAppContext, TIdentity, TCriteria, TItem> : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
        where TItem : QueryResultItemBase
    {
        /// <inheritdoc />
        protected QueryListWithCriteriaUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Runs the query.
        /// </summary>
        /// <param name="criteria">the criteria</param>
        /// <param name="cancellationToken">a token to cancel the query</param>
        /// <returns>the result</returns>
        public Task<List<TItem>> QueryAsync(TCriteria criteria, CancellationToken cancellationToken = default)
        {
            if (null == criteria) throw new ArgumentNullException(nameof(criteria));

            return RunReadOnlyAsync((dp, ct) => dp.FetchAsync<TCriteria, List<TItem>>(GetType(), criteria, ct),
                ct => DoQueryAsync(criteria, ct), cancellationToken, criteria);
        }

        /// <summary>
        /// Gets the metadata of the items, for the grid. Local.
        /// </summary>
        public IEntityMetadata GetResultMetadata() => GetMetadataFor<TItem>();


        /// <summary>
        /// Runs the query.
        /// </summary>
        /// <param name="criteria">the criteria, as received: an app merges them into a trusted instance</param>
        /// <param name="cancellationToken">a token to cancel the query</param>
        protected abstract Task<List<TItem>> DoQueryAsync(TCriteria criteria, CancellationToken cancellationToken);
    }
}
