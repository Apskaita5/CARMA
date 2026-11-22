using System;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// Translates the application exceptions to remote errors on the server and back to
    /// exceptions on the client. Implemented by the application, because its business
    /// exceptions (and their localized messages) are application types.
    /// </summary>
    public interface IRemoteErrorMapper
    {
        /// <summary>
        /// Server side: gets a remote error for the exception, or null if the exception is
        /// not one the client should know anything about. A null is reported to the client
        /// as <see cref="RemoteErrorKind.ServerError"/> with a correlation id only.
        /// </summary>
        /// <param name="exception">an exception thrown by a use case</param>
        /// <remarks>Map the business exceptions, and a transient exception that has survived
        /// the server side retries to <see cref="RemoteErrorKind.ServiceUnavailable"/>.
        /// Never copy a message of an exception that is not a business exception.</remarks>
        RemoteError ToError(Exception exception);

        /// <summary>
        /// Client side: gets an exception to throw for the remote error, or null to throw
        /// a <see cref="RemoteCallException"/>.
        /// </summary>
        /// <param name="error">an error received from the server (or created by the client
        /// data portal when the response itself is unusable)</param>
        Exception ToException(RemoteError error);
    }
}
