using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application.Authorization.Authorizers
{
    /// <summary>
    /// The user has logged in, but has none of the permissions required. Reported across the data portal as
    /// <see cref="DataPortal.RemoteErrorKind.Unauthorized"/>, unless the app's error mapper says otherwise.
    /// </summary>
    /// <remarks>The actual exception is shown to a user therefore requires localization which cannot be provided by the framework.</remarks>
    public abstract class NotAuthorizedExceptionBase : BusinessException
    {
        /// <summary>Creates the exception.</summary>
        protected NotAuthorizedExceptionBase() : base("I'm sorry, Dave. I'm afraid I can't do that.") { }

        /// <summary>Creates the exception.</summary>
        protected NotAuthorizedExceptionBase(string message) : base(message) { }

        /// <summary>Creates the exception.</summary>
        protected NotAuthorizedExceptionBase(string message, Exception innerException) : base(message, innerException) { }
    }
}
