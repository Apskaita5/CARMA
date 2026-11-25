using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Application.Retry;
using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Creates the app's localized business exceptions the framework needs: a service that is temporarily
    /// unavailable, and a client that does not match the server. Implemented by the app, because the messages are
    /// localized; called in the current UI culture (on the server, the caller's).
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IBusinessExceptionFactory
    {
        /// <summary>
        /// Gets the business exception that a transient failure becomes at the use case boundary, e.g. "The mail
        /// service is temporarily unavailable, please try again in 5 minutes".
        /// </summary>
        /// <param name="transient">the transient failure: its <see cref="TransiantException.ServiceName"/> and
        /// <see cref="TransiantException.RetryAfter"/> make the message. A
        /// <see cref="DataPortalUnavailableException"/> is the application server, which only the app can name.</param>
        /// <param name="thrown">the exception thrown, which holds the transient one: pass it as the inner exception,
        /// so that retry logic still finds the transient one</param>
        /// <returns>a business exception, e.g. the app's ServiceUnavailableException</returns>
        BusinessException ServiceUnavailable(TransiantException transient, Exception thrown);

        /// <summary>
        /// Client side: gets the business exception for a client that does not match the server, stating what the
        /// user should do (e.g. "A new version is available: restart the application").
        /// </summary>
        /// <param name="data">the details of the mismatch</param>
        /// <param name="innerException">the cause, for the log; pass it as the inner exception</param>
        BusinessException VersionMismatch(VersionMismatchData data, Exception innerException);
    }
}
