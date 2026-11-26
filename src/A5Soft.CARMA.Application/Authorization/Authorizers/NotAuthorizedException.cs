using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application.Authorization.Authorizers
{
    /// <summary>
    /// The user has logged in, but has none of the permissions required.
    /// Reported across the data portal as a business exception (RemoteErrorKind.Business): the client gets its message only.
    /// </summary>
    /// <remarks>The actual exception is shown to a user therefore requires localization which cannot be provided by the framework.
    /// Create extension methods for throwing with localized messages in common scenarios.</remarks>
    public class NotAuthorizedException : BusinessException
    {
        /// <summary>Creates the exception.</summary>
        /// <param name="message">a message for the user, localized</param>
        public NotAuthorizedException(string message) : base(message) { }

        /// <summary>Creates the exception.</summary>
        /// <param name="message">a message for the user, localized</param>
        /// <param name="innerException">the cause, for the log only</param>
        public NotAuthorizedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
