using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Entities
{
    /// <summary>
    /// Fetches the domain singleton (settings, the company profile). Does not change state, so it can be cancelled.
    /// Interface method: <c>Task&lt;TSingleton&gt; FetchAsync(CancellationToken cancellationToken = default)</c>.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TSingleton">the type of the singleton</typeparam>
    public abstract class FetchSingletonUseCaseBase<TAppContext, TIdentity, TSingleton> : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
        where TSingleton : class
    {
        /// <inheritdoc />
        protected FetchSingletonUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Fetches the singleton.
        /// </summary>
        /// <param name="cancellationToken">a token to cancel the fetch</param>
        /// <returns>the singleton</returns>
        public Task<TSingleton> FetchAsync(CancellationToken cancellationToken = default)
            => RunReadOnlyAsync((dp, ct) => dp.FetchAsync<TSingleton>(GetType(), ct),
                async ct => await AfterFetchAsync(await DoFetchAsync(ct).ConfigureAwait(false), ct)
                    .ConfigureAwait(false),
                cancellationToken);

        /// <summary>
        /// Gets the metadata of the singleton, for the form. Local.
        /// </summary>
        public IEntityMetadata GetMetadata() => GetMetadataFor<TSingleton>();


        /// <summary>
        /// Fetches the singleton; one with the defaults if none is persisted yet.
        /// </summary>
        /// <param name="cancellationToken">a token to cancel the fetch</param>
        protected abstract Task<TSingleton> DoFetchAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Runs after the fetch, e.g. to hide the audit trace from a user who may not see it.
        /// </summary>
        /// <param name="singleton">the singleton fetched</param>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        /// <returns>the singleton to return</returns>
        protected virtual Task<TSingleton> AfterFetchAsync(TSingleton singleton, CancellationToken cancellationToken)
            => Task.FromResult(singleton);
    }
}
