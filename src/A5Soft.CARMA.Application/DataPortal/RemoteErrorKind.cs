namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// The kinds of failure a remote call can report to the client.
    /// </summary>
    public enum RemoteErrorKind
    {
        /// <summary>An unexpected server failure. The client gets a correlation id only.</summary>
        ServerError = 0,

        /// <summary>A business exception; the message is shown to the user as it is.</summary>
        Business = 1,

        /// <summary>Invalid data; the message and the broken rules are sent.</summary>
        Validation = 2,

        /// <summary>Somebody else has changed the data.</summary>
        Concurrency = 3,

        /// <summary>The caller has logged in, but is not allowed to invoke the use case.</summary>
        Unauthorized = 4,

        /// <summary>A service behind the server (a database, a mail server) is temporarily unavailable: the
        /// app's error mapper reports a <see cref="Retry.TransiantException"/> so. The client's own failure to
        /// reach the server is not a remote error: it is thrown as <see cref="DataPortalUnavailableException"/>.</summary>
        ServiceUnavailable = 5,

        /// <summary>The call was rejected before execution: a malformed call, a payload or upload over
        /// the limits, or an answer the client does not understand.</summary>
        BadRequest = 6,

        /// <summary>
        /// The client and the server are different applications, or versions of the application that
        /// do not match, or speak different versions of the transport protocol. Detected by the client
        /// before the call is sent; the <see cref="RemoteError.Data"/> holds a
        /// <see cref="VersionMismatchData"/>. The user must update (reload) the client.
        /// </summary>
        VersionMismatch = 7,

        /// <summary>
        /// The caller has not logged in, or the session has ended (logout, expiry, a server restart): log in again.
        /// </summary>
        Unauthenticated = 8
    }
}
