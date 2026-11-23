using System;
using System.Threading;

namespace A5Soft.CARMA.Application.DataPortal.Http
{
    /// <summary>
    /// The options of the HTTP transport on the client.
    /// </summary>
    /// <remarks>Implemented by <see cref="HttpDataPortalClientOptions"/>, read from the app configuration
    /// (section <c>HttpDataPortalClientOptions</c>).</remarks>
    public interface IHttpDataPortalClientOptions
    {
        /// <summary>
        /// Gets a max length (in characters) of any single request header, checked before sending. Reverse
        /// proxies commonly refuse headers over 8 KB.
        /// </summary>
        int MaxRequestHeaderLength { get; }

        /// <summary>
        /// Gets a max length (in characters) of all the request headers together, checked before sending.
        /// IIS refuses more than 16 KB of request line and headers.
        /// </summary>
        int MaxRequestHeadersLength { get; }

        /// <summary>
        /// Gets whether a browser (Blazor WebAssembly) should stream request bodies instead of buffering them
        /// in memory. Requires .NET 7 or later in the browser, a Chromium browser and HTTP/2 to the server.
        /// Ignored outside a browser.
        /// </summary>
        bool BrowserRequestStreaming { get; }

        /// <summary>
        /// Gets the longest a call may take until the response headers arrive, the upload included (a download is
        /// read afterwards); <see cref="Timeout.InfiniteTimeSpan"/> for no limit. Default: 100 seconds.
        /// </summary>
        TimeSpan CallTimeout { get; }
    }
}
