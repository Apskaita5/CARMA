using A5Soft.CARMA.Application.Retry;
using System;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// Thrown by the client data portal when the data portal server is unavailable: it could not be reached, did not
    /// answer in time, the connection was lost, or a gateway in front of it answered that it is unavailable.
    /// </summary>
    /// <remarks>
    /// <para>Only for the data portal's own failures. An unavailable service behind the server (a database, a mail
    /// server) is reported by the server as <see cref="RemoteErrorKind.ServiceUnavailable"/>, and thrown as the app's
    /// <see cref="IRemoteErrorMapper.ToException"/> maps it.</para>
    /// <para>The message is for the user (<see cref="IRemoteErrorMapper.GetDataPortalUnavailableMessage"/>), e.g.
    /// "The server is temporarily unavailable, please try again later". The use cases do not retry: the user repeats
    /// the action, and a background service retries the use case itself, if it should (it is a
    /// <see cref="TransiantException"/>).</para>
    /// <para>A call that changes state may have reached the server and been executed before the failure (e.g. the
    /// answer did not arrive in time): check before repeating it.</para>
    /// </remarks>
    public class DataPortalUnavailableException : TransiantException
    {
        private const string FallbackMessage = "The server is temporarily unavailable. Please try again later.";


        /// <summary>
        /// Creates a new exception.
        /// </summary>
        /// <param name="message">a message for the user, in the current UI culture; null or empty for a fallback
        /// in English</param>
        /// <param name="innerException">the transport failure, if any</param>
        public DataPortalUnavailableException(string message, Exception innerException = null)
            : base(string.IsNullOrWhiteSpace(message) ? FallbackMessage : message, innerException) { }
    }
}
