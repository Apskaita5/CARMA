using System;

namespace A5Soft.CARMA.Application.Retry
{
    /// <summary>
    /// Base interface for transient exception validation,
    /// i.e. deciding whether a particular exception is transient.
    /// </summary>
    public interface ITransientExceptionValidator
    {
        /// <summary>
        /// Gets a value indicating whether the exception specified is transient.
        /// </summary>
        /// <param name="ex">an exception to validate (not null)</param>
        /// <returns>a value indicating whether the exception specified is transient</returns>
        bool IsTransient(Exception ex);
    }
}
