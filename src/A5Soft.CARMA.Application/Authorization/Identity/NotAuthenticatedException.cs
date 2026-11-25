using System;

namespace A5Soft.CARMA.Application.Authorization.Identity
{
    /// <summary>
    /// The user has not logged in, or the session has ended: log in again. Reported across the data portal as
    /// <see cref="DataPortal.RemoteErrorKind.Unauthenticated"/>, unless the app's error mapper says otherwise.
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
