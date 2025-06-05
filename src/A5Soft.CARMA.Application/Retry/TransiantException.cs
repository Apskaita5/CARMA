using System;

namespace A5Soft.CARMA.Application.Retry
{
    /// <summary>
    /// Wrapper for transient exceptions (e.g. remote database, mail or web server temporarily offline).
    /// </summary>
    [Serializable]
    public class TransiantException : Exception
    {
        public TransiantException() : base() { }

        public TransiantException(string message) : base(message) { }

        public TransiantException(string message, Exception innerException) : base(message, innerException) { }

        public TransiantException(Exception innerException) : base(innerException?.Message ?? string.Empty, innerException) { }
    }
}
