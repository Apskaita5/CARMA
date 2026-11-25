using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// Translates the application exceptions to remote errors on the server and back to
    /// exceptions on the client. Implemented by the application, because its business
    /// exceptions (and their localized messages) are application types.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IRemoteErrorMapper
    {
        /// <summary>
        /// Server side: gets a remote error for the exception, or null if the exception is
        /// not one the client should know anything about. A null is reported to the client
        /// as <see cref="RemoteErrorKind.ServerError"/> with a correlation id only.
        /// </summary>
        /// <param name="exception">an exception thrown by a use case</param>
        /// <remarks>Map the business exceptions, and a <see cref="Retry.TransiantException"/> (a database or
        /// a mail server is unavailable) to <see cref="RemoteErrorKind.ServiceUnavailable"/> with a message
        /// for the user. Never copy a message of an exception that is not a business exception.</remarks>
        RemoteError ToError(Exception exception);

        /// <summary>
        /// Client side: gets an exception to throw for the remote error, or null to throw
        /// a <see cref="RemoteCallException"/>.
        /// </summary>
        /// <param name="error">an error received from the server (or created by the client
        /// data portal when the response itself is unusable)</param>
        /// <remarks><see cref="RemoteErrorKind.ServiceUnavailable"/> is a service behind the server: map it to
        /// a <see cref="Retry.TransiantException"/> (with the error's message), so that a background service
        /// can tell that it may retry. Never to a <see cref="DataPortalUnavailableException"/>: that one is
        /// for the data portal server itself.</remarks>
        Exception ToException(RemoteError error);

        /// <summary>
        /// Client side: gets the message of a <see cref="DataPortalUnavailableException"/>, for the user, in
        /// the current UI culture: the data portal server cannot be reached, e.g. "The server is temporarily
        /// unavailable, please try again later".
        /// </summary>
        string GetDataPortalUnavailableMessage();
    }
}
