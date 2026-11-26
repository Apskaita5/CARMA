using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Queries
{
    /// <summary>
    /// A query returning a single result. Does not change state, so it can be cancelled.
    /// Interface method: <c>Task&lt;TResult&gt; QueryAsync(CancellationToken cancellationToken = default)</c>.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TResult">the type of the result</typeparam>
    public abstract class QueryUseCaseBase<TAppContext, TIdentity, TResult> : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
        where TResult : QueryResultBase
    {
        /// <inheritdoc />
        protected QueryUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Runs the query.
        /// </summary>
        /// <param name="cancellationToken">a token to cancel the query</param>
        /// <returns>the result</returns>
        public Task<TResult> QueryAsync(CancellationToken cancellationToken = default)
        {
            // cached on the client only if the result type says so
            return RunCachedReadOnlyAsync(typeof(TResult),
                ClientCacheLifetimeAttribute.GetLifetime(typeof(TResult)) ?? TimeSpan.Zero, null,
                (dp, ct) => dp.FetchAsync<TResult>(GetType(), ct),
                DoQueryAsync, cancellationToken);
        }

        /// <summary>
        /// Gets the metadata of the result, for the form. Local.
        /// </summary>
        public IEntityMetadata GetResultMetadata() => GetMetadataFor<TResult>();


        /// <summary>
        /// Runs the query.
        /// </summary>
        /// <param name="cancellationToken">a token to cancel the query</param>
        protected abstract Task<TResult> DoQueryAsync(CancellationToken cancellationToken);
    }
}
