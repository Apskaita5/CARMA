using System;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A serializable wrapper for the data portal result.
    /// </summary>
    [Serializable]
    internal class DataPortalResponse
    {

        /// <summary>
        /// Gets or sets an exception that was thrown while processing the request on server (if any).
        /// </summary>
        public Exception ProcessingException { get; set; } = null;

        /// <summary>
        /// Gets or sets a result of the method invocation on server (if the method is not void).
        /// </summary>
        public object Result { get; set; } = null;

    }
}
