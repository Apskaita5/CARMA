using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// Gets the <see cref="ServerSettings"/> of the server the client data portal calls, fetching them
    /// once and keeping them.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IServerSettingsProvider
    {
        /// <summary>
        /// Gets the server settings, fetching them through the proxy on first use.
        /// </summary>
        /// <param name="cancellationToken">a token to stop waiting; it never cancels a fetch other
        /// callers are waiting for</param>
        /// <returns>the settings, checked to be usable</returns>
        /// <exception cref="DataPortalTransportException">the settings could not be fetched, or are not
        /// usable; a failure is not kept, so the next call tries again</exception>
        Task<ServerSettings> GetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Forgets the settings, so that they are fetched again on next use: e.g. after the server has
        /// answered with something the client does not understand, which usually means that the server
        /// has been updated.
        /// </summary>
        void Invalidate();
    }
}
