using System;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// Thrown by the client data portal for a remote error that the application error
    /// mapper has not translated to an exception of its own.
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
