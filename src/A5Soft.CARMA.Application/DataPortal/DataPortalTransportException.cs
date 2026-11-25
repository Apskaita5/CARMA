using System;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// Thrown by an <see cref="IDataPortalProxy"/> when it has not received a data portal response:
    /// the server is unreachable, something other than the data portal answered (a gateway, a proxy,
    /// a login page), or the response is malformed.
    /// </summary>
    /// <remarks>The client data portal throws a <see cref="DataPortalUnavailableException"/> for
    /// <see cref="RemoteErrorKind.ServiceUnavailable"/>, and reports any other kind as a <see cref="RemoteError"/>
    /// of that kind; either way with this exception as the cause.</remarks>
    public class DataPortalTransportException : Exception
    {
        /// <summary>
        /// Creates a new exception.
        /// </summary>
        /// <param name="kind">the kind of error to report: usually
        /// <see cref="RemoteErrorKind.ServiceUnavailable"/> or <see cref="RemoteErrorKind.ServerError"/>,
        /// <see cref="RemoteErrorKind.BadRequest"/> for a malformed response</param>
        /// <param name="message">a description of the failure</param>
        /// <param name="innerException">the transport failure, if any</param>
        public DataPortalTransportException(RemoteErrorKind kind, string message, Exception innerException = null)
            : base(message, innerException)
        {
            Kind = kind;
        }

        /// <summary>Gets the kind of error to report.</summary>
        public RemoteErrorKind Kind { get; }
    }
}
