using System;
using System.Runtime.Serialization;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// An exception the user resolves, or an administrator: its localized message says what happened and, where
    /// relevant, what to do. At GUI level nothing else matters; inner exceptions, custom properties and the stack
    /// trace are for the log only.
    /// </summary>
    /// <remarks>
    /// <para>Concrete, so that it can be thrown when no subtype is needed and re-created on the client from the
    /// message the server sent. Only the message crosses the data portal: a subtype reaches the client as a plain
    /// <see cref="BusinessException"/>.</para>
    /// <para>Recognize it with <see cref="Extensions.ToBusinessException"/>, which searches the whole exception
    /// hierarchy, rather than with a typed catch.</para>
    /// </remarks>
    [Serializable]
    public class BusinessException : Exception
    {
        /// <summary>
        /// Creates a business exception.
        /// </summary>
        /// <param name="message">a message for the user, localized</param>
        public BusinessException(string message) : base(message) { }

        /// <summary>
        /// Creates a business exception.
        /// </summary>
        /// <param name="message">a message for the user, localized</param>
        /// <param name="innerException">the cause, for the log only</param>
        public BusinessException(string message, Exception innerException) : base(message, innerException) { }

        /// <inheritdoc />
        protected BusinessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
