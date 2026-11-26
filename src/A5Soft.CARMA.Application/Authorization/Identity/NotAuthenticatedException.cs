using System;

namespace A5Soft.CARMA.Application.Authorization.Identity
{
    /// <summary>
    /// The user has not logged in, or the session has ended: the GUI redirects to the login.
    /// Not a business exception, so that nothing shows it as a message; its message is for the log only.
    /// Reported across the data portal as RemoteErrorKind.Unauthenticated, without the message.
    /// </summary>
    public class NotAuthenticatedException : Exception
    {
        /// <summary>Creates the exception.</summary>
        public NotAuthenticatedException() : base("Log in first.") { }

        /// <summary>Creates the exception.</summary>
        public NotAuthenticatedException(string message) : base(message) { }

        /// <summary>Creates the exception.</summary>
        public NotAuthenticatedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
