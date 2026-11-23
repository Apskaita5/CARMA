using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.DataPortal.EndPoints;
using A5Soft.CARMA.Application.DataPortal.Protocol;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.DataPortal.Client
{
    /// <summary>
    /// Gets the <see cref="ServerSettings"/> of a server, fetching them once per endpoint and keeping them.
    /// Server settings are facts about a server, not user state, so one provider serves every user and scope;
    /// e.g. a login screen checks a server's version and plugins before anything else is sent to it.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IServerSettingsProvider
    {
        /// <summary>
        /// Gets the settings of a server, fetching them through its proxy on first use.
        /// </summary>
        /// <param name="endpoint">the server's endpoint</param>
        /// <param name="cancellationToken">a token to stop waiting; it never cancels a fetch other
        /// callers are waiting for</param>
        /// <returns>the settings, checked to be usable</returns>
        /// <exception cref="DataPortalTransportException">the settings could not be fetched, or are not
        /// usable; a failure is not kept, so the next call tries again</exception>
        /// <exception cref="ArgumentException">an unknown transport, or an address missing or not expected</exception>
        /// <exception cref="InvalidOperationException">the endpoint is local: there is no server</exception>
        Task<ServerSettings> GetAsync(DataPortalEndpoint endpoint, CancellationToken cancellationToken = default);

        /// <summary>
        /// Forgets the settings of a server, so that they are fetched again on next use: e.g. after the server
        /// has answered with something the client does not understand, which usually means that the server
        /// has been updated.
        /// </summary>
        /// <param name="endpoint">the server's endpoint</param>
        void Invalidate(DataPortalEndpoint endpoint);
    }
}
