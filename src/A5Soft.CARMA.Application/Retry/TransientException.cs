using System;

namespace A5Soft.CARMA.Application.Retry
{
    /// <summary>
    /// An external service is temporarily unavailable: a database, a mail gateway, a file store, an external API, or,
    /// seen from the client, the application server (<see cref="DataPortal.DataPortalUnavailableException"/>).
    /// </summary>
    /// <remarks>
    /// <para>Thrown only by repositories and infrastructure services, which decide which provider exceptions are
    /// transient (<see cref="ITransientExceptionValidator"/>); never by a use case. It serves retry logic: at the use
    /// case boundary it is wrapped into the app's business exception
    /// (<see cref="IExceptionFactory.ServiceUnavailable"/>), which the retry logic still finds it in
    /// (<see cref="Extensions.ToTransientException"/>).</para>
    /// <para>Its message is for the log only.</para>
    /// </remarks>
    [Serializable]
    public class TransientException : Exception
    {
        /// <summary>Creates the exception.</summary>
        public TransientException() : base() { }

        /// <summary>Creates the exception.</summary>
        /// <param name="message">a description for the log</param>
        public TransientException(string message) : base(message) { }

        /// <summary>Creates the exception.</summary>
        /// <param name="message">a description for the log</param>
        /// <param name="innerException">the provider's exception</param>
        public TransientException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>Creates the exception.</summary>
        /// <param name="innerException">the provider's exception</param>
        public TransientException(Exception innerException)
            : base(innerException?.Message ?? string.Empty, innerException) { }

        /// <summary>
        /// Creates the exception for a service the user recognizes.
        /// </summary>
        /// <param name="serviceName">the service as the user knows it (e.g. "the mail service"), never a host name
        /// or a connection string</param>
        /// <param name="retryAfter">when to try again, if the service says</param>
        /// <param name="message">a description for the log</param>
        /// <param name="innerException">the provider's exception</param>
        public TransientException(string serviceName, TimeSpan? retryAfter, string message,
            Exception innerException = null) : base(message, innerException)
        {
            ServiceName = serviceName;
            RetryAfter = retryAfter;
        }


        /// <summary>
        /// Gets the unavailable service as the user knows it; null if the thrower did not say.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        /// Gets when to try again, if the service has said; otherwise null.
        /// </summary>
        public TimeSpan? RetryAfter { get; }
    }
}
