using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Entities
{
    /// <summary>
    /// Updates the domain singleton: fetches the persisted one, merges the one received into it and persists the
    /// result.
    /// </summary>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TSingleton">the type of the singleton</typeparam>
    public abstract class UpdateSingletonUseCaseBase<TAppContext, TIdentity, TSingleton> : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
        where TSingleton : class
    {
        /// <inheritdoc />
        protected UpdateSingletonUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Updates the singleton.
        /// </summary>
        /// <param name="singleton">the singleton as edited</param>
        /// <returns>the singleton as persisted</returns>
        public Task<TSingleton> UpdateAsync(TSingleton singleton)
        {
            if (null == singleton) throw new ArgumentNullException(nameof(singleton));

            return RunAsync(dp => dp.FetchAsync<TSingleton, TSingleton>(GetType(), singleton),
                () => UpdateLocallyAsync(singleton), singleton);
        }

        /// <summary>
        /// Gets the metadata of the singleton, for the form. Local.
        /// </summary>
        public IEntityMetadata GetMetadata() => GetMetadataFor<TSingleton>();


        /// <summary>
        /// Fetches the persisted singleton; one with the defaults if none is persisted yet.
        /// </summary>
        protected abstract Task<TSingleton> FetchPersistedAsync();

        /// <summary>
        /// Merges the singleton received into the persisted one: <c>persisted.Merge(untrusted, true)</c>.
        /// </summary>
        /// <param name="persisted">the persisted singleton</param>
        /// <param name="untrusted">the singleton as received</param>
        protected abstract void Merge(TSingleton persisted, TSingleton untrusted);

        /// <summary>
        /// Persists the merged singleton and returns it as persisted.
        /// </summary>
        /// <param name="merged">the persisted singleton with the changes merged in</param>
        protected abstract Task<TSingleton> DoUpdateAsync(TSingleton merged);


        private async Task<TSingleton> UpdateLocallyAsync(TSingleton untrusted)
        {
            var persisted = await FetchPersistedAsync().ConfigureAwait(false);
            Merge(persisted, untrusted);
            return await DoUpdateAsync(persisted).ConfigureAwait(false);
        }
    }
}
