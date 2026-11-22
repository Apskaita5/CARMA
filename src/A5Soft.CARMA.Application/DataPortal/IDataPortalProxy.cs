using System.Threading;
using System.Threading.Tasks;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A transport underneath the client data portal. Translates a canonical request into its
    /// transport, and what it receives back into a canonical response. Knows nothing about use cases.
    /// </summary>
    /// <remarks>
    /// An implementation must:
    /// <list type="bullet">
    /// <item>deliver <see cref="DataPortalRequest.Call"/> before the data, so the server can refuse a
    /// call without receiving its files, and send the payload, then the files in order;</item>
    /// <item>read each file once, never dispose it, and resend a call only while
    /// <see cref="DataPortalRequest.FilesRead"/> is false;</item>
    /// <item>return a response as soon as it is known, with a downloaded file still arriving: the
    /// file's stream must release the transport resources when it is disposed;</item>
    /// <item>throw a <see cref="DataPortalTransportException"/> when no data portal response was
    /// received, including when reading a file failed (the client data portal checks its own limits
    /// first);</item>
    /// <item>get the server settings without authentication, as <see cref="GetServerSettingsAsync"/>
    /// is called before the first call, and logging in is a call;</item>
    /// <item>let any other exception (a configuration or programming error) propagate.</item>
    /// </list>
    /// </remarks>
    [Service(ServiceLifetime.Scoped)]
    public interface IDataPortalProxy
    {
        /// <summary>
        /// Gets a value indicating whether use cases run remotely. False for the local proxy,
        /// in which case the data portal is never called.
        /// </summary>
        bool IsRemote { get; }

        /// <summary>
        /// Sends a call and gets its response.
        /// </summary>
        /// <param name="request">a call to send</param>
        /// <param name="cancellationToken">a token to cancel the call</param>
        /// <returns>the response; the caller disposes it</returns>
        /// <exception cref="DataPortalTransportException">no data portal response was received</exception>
        Task<DataPortalResponse> SendAsync(DataPortalRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the public settings of the server. Called once, before the first call, by the
        /// <see cref="IServerSettingsProvider"/>; must not require authentication.
        /// </summary>
        /// <param name="cancellationToken">a token to cancel the request</param>
        /// <returns>the settings as received; the caller checks them</returns>
        /// <exception cref="DataPortalTransportException">no settings were received; of kind
        /// <see cref="RemoteErrorKind.VersionMismatch"/> if the server speaks another version of the
        /// transport protocol</exception>
        Task<ServerSettings> GetServerSettingsAsync(CancellationToken cancellationToken = default);
    }
}
