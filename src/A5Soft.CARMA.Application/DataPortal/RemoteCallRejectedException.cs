using System;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// Thrown inside the data portal when a call, a value or an upload does not match the contract
    /// or the limits. The message is for the log only; it is never sent to the other side.
    /// </summary>
    public class RemoteCallRejectedException : Exception
    {
        /// <summary>
        /// Creates a new exception.
        /// </summary>
        /// <param name="kind">a kind of error to report to the client</param>
        /// <param name="logMessage">a description of the reason, for the log only</param>
        /// <param name="innerException">a cause, if any</param>
        public RemoteCallRejectedException(RemoteErrorKind kind, string logMessage,
            Exception innerException = null)
            : base(logMessage, innerException)
        {
            Kind = kind;
        }

        /// <summary>Gets a kind of error to report to the client.</summary>
        public RemoteErrorKind Kind { get; }
    }
}
