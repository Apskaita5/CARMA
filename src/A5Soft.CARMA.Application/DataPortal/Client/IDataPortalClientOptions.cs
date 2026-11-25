namespace A5Soft.CARMA.Application.DataPortal.Client
{
    /// <summary>
    /// The options of the client data portal: the settings that are the client's own. The limits a call
    /// must respect come from the server, in the <see cref="Protocol.ServerSettings"/>.
    /// </summary>
    /// <remarks>Implemented by <see cref="DataPortalClientOptions"/>, read from the app configuration
    /// (section <c>DataPortalClientOptions</c>). Every setting has a default, so a client usually needs no
    /// configuration at all.</remarks>
    public interface IDataPortalClientOptions
    {
        /// <summary>Gets how strictly the client's application version is compared with the server's.</summary>
        AppVersionMatch AppVersionMatch { get; }

        /// <summary>
        /// Gets a max length (in bytes) of a refreshed identity, in the form a transport carries it, that the
        /// client accepts.
        /// </summary>
        int MaxIdentityLength { get; }

        /// <summary>
        /// Gets a max length (in bytes) of a JSON result that the client accepts. The result is held in
        /// memory. Downloaded files are not limited.
        /// </summary>
        int MaxResponseLength { get; }

        /// <summary>Gets a size (in bytes) of the buffers used to copy files on the client.</summary>
        int CopyBufferSize { get; }
    }
}
