namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// The class of a failed remote call, as the server sends it to the client: only the class and, for a business
    /// exception, its localized message cross the wire.
    /// </summary>
    /// <remarks>The values not listed (2 validation, 3 concurrency, 4 unauthorized) are no longer used: those are
    /// business exceptions.</remarks>
    public enum RemoteErrorKind
    {
        /// <summary>A fault: what the server handled nowhere. The client gets a correlation id only, and throws a
        /// <see cref="RemoteCallException"/>.</summary>
        ServerError = 0,
        /// <summary>A business exception; its localized message is re-created on the client as a
        /// <see cref="Domain.BusinessException"/>. Authorization failures and transient failures behind the server
        /// arrive as this too.</summary>
        Business = 1,
        /// <summary>Transport only, never sent by the server: the proxy could not reach the server. The client data
        /// portal throws a <see cref="DataPortalUnavailableException"/>.</summary>
        ServiceUnavailable = 5,
        /// <summary>The call was rejected before execution: a malformed call, a payload or upload over the limits,
        /// or an answer the client does not understand. A fault.</summary>
        BadRequest = 6,
        /// <summary>
        /// The client and the server are different applications, or versions of the application that do not match,
        /// or speak different versions of the transport protocol. The client throws the app's business exception
        /// (<see cref="IExceptionFactory.VersionMismatch"/>): the user must update (reload) the client.
        /// </summary>
        VersionMismatch = 7,
        /// <summary>
        /// The caller has not logged in, or the session has ended (logout, expiry, a server restart): the client
        /// throws a <see cref="Authorization.Identity.NotAuthenticatedException"/>.
        /// </summary>
        Unauthenticated = 8
    }
}
