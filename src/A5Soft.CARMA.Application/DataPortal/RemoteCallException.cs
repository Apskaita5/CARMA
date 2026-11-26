using System;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A fault of a remote call (ServerError or BadRequest): handled by no one.
    /// The GUI shows its generic internal error message; the server has logged the details under the RemoteError.CorrelationId.
    /// </summary>
    public class RemoteCallException : Exception
    {
        /// <summary>
        /// Creates a new exception for the remote error given.
        /// </summary>
        /// <param name="error">a remote error</param>
        /// <param name="innerException">a local cause, e.g. the transport failure that kept the
        /// call from reaching the server; null for an error reported by the server</param>
        public RemoteCallException(RemoteError error, Exception innerException = null)
            : base(string.IsNullOrWhiteSpace(error?.Message)
                ? $"Remote call has failed ({error?.Kind}, correlation id: {error?.CorrelationId})."
                : error.Message, innerException)
        {
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }

        /// <summary>Gets the remote error.</summary>
        public RemoteError Error { get; }

        /// <summary>Gets a kind of the failure.</summary>
        public RemoteErrorKind Kind => Error.Kind;
    }
}
