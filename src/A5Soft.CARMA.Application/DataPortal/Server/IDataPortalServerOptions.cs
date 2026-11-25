namespace A5Soft.CARMA.Application.DataPortal.Server
{
    /// <summary>
    /// The options of the server data portal. The limits among them are published to the clients in the
    /// <see cref="Protocol.ServerSettings"/>, so they are configured on the server only.
    /// </summary>
    /// <remarks>Implemented by <see cref="DataPortalServerOptions"/>, read from the app configuration
    /// (section <c>DataPortalServerOptions</c>).</remarks>
    public interface IDataPortalServerOptions
    {
        /// <summary>Gets a max number of files uploaded by a single call. Published to the clients.</summary>
        int MaxFileCount { get; }

        /// <summary>Gets a max length (in bytes) of a single uploaded file. Published to the clients.</summary>
        long MaxFileLength { get; }

        /// <summary>Gets a max length (in bytes) of all the files uploaded by a single call. Published to
        /// the clients.</summary>
        long MaxUploadLength { get; }

        /// <summary>
        /// Gets a max length (in bytes) of the JSON payload of a call, i.e. of its parameters with the files
        /// replaced by references. The payload is held in memory. Published to the clients.
        /// </summary>
        int MaxPayloadLength { get; }

        /// <summary>Gets a max nesting depth of the JSON written and read by both sides. Published to the
        /// clients.</summary>
        int MaxDepth { get; }

        /// <summary>Gets a size (in bytes) of the buffers used to copy files on the server.</summary>
        int CopyBufferSize { get; }

        /// <summary>
        /// Gets a directory for the temp files that uploaded files are spooled to; null for the system temp
        /// directory. The files are deleted when they are closed.
        /// </summary>
        string SpoolDirectory { get; }
    }
}
