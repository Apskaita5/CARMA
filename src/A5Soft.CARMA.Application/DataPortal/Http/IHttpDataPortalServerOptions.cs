namespace A5Soft.CARMA.Application.DataPortal.Http
{
    /// <summary>
    /// The options of the HTTP endpoint of the server data portal.
    /// </summary>
    /// <remarks>Implemented by <see cref="HttpDataPortalServerOptions"/>, read from the app configuration
    /// (section <c>HttpDataPortalServerOptions</c>). Optional: without a registration, the defaults apply.</remarks>
    public interface IHttpDataPortalServerOptions
    {
        /// <summary>
        /// Gets a max length (in characters) of any single data portal request header the endpoint reads.
        /// A longer header refuses the call.
        /// </summary>
        int MaxRequestHeaderLength { get; }

        /// <summary>
        /// Gets a max length (in bytes) of the headers of a single multipart part.
        /// </summary>
        int MaxPartHeadersLength { get; }
    }
}
