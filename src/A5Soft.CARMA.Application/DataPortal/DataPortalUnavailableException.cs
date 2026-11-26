using A5Soft.CARMA.Application.Retry;
using System;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// Thrown by the client data portal when the data portal server is unavailable: it could not be reached, did not
    /// answer in time, the connection was lost, or a gateway in front of it answered that it is unavailable.
    /// </summary>
    /// <remarks>
    /// <para>A <see cref="TransientException"/>, the application server seen from the client. The use case boundary
    /// wraps it into the app's business exception (<see cref="IExceptionFactory.ServiceUnavailable"/>,
    /// which names the server as the app calls it), so the user reads the business exception's message; its own
    /// message is for the log.</para>
    /// <para>Only for the data portal's own failures. A service behind the server that is unavailable has been
    /// wrapped on the server already, and reaches the client as a business exception.</para>
    /// <para>A call that changes state may have reached the server and been executed before the failure (e.g. the
    /// answer did not arrive in time): check before repeating it.</para>
    /// </remarks>
    public class DataPortalUnavailableException : TransientException
    {
        /// <summary>
        /// A fixed name for the data portal server.
        /// </summary>
        public const string DataPortalName = "data portal server";


        /// <summary>
        /// Creates a new exception.
        /// </summary>
        /// <param name="innerException">the transport failure, if any</param>
        /// <param name="retryAfter">when to try again, if the server or a gateway has said</param>
        public DataPortalUnavailableException(Exception innerException = null, TimeSpan? retryAfter = null)
            : base(DataPortalName, retryAfter, "The data portal server is unavailable.", innerException) { }
    }
}
