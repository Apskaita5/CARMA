using Newtonsoft.Json.Linq;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A failure of a remote call as it is sent to the client.
    /// Exceptions themselves never cross the data portal.
    /// </summary>
    public sealed class RemoteError
    {
        /// <summary>Gets or sets a kind of the failure.</summary>
        public RemoteErrorKind Kind { get; set; }

        /// <summary>
        /// Gets or sets a message for the user, already localized for the caller's culture.
        /// Empty for <see cref="RemoteErrorKind.ServerError"/> and
        /// <see cref="RemoteErrorKind.BadRequest"/>: the client supplies its own text for those.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets an id under which the server has logged the details of the failure.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets application defined details, e.g. the broken rules of a validation failure.
        /// </summary>
        public JToken Data { get; set; }
    }
}
