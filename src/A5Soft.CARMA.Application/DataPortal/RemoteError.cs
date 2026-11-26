namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A failure of a remote call as it is sent to the client: its class and, for a business exception, the
    /// localized message. Exceptions themselves never cross the data portal; the server has logged the whole tree.
    /// </summary>
    public sealed class RemoteError
    {
        /// <summary>Gets or sets the class of the failure.</summary>
        public RemoteErrorKind Kind { get; set; }

        /// <summary>
        /// Gets or sets the message for the user of a <see cref="RemoteErrorKind.Business"/> failure, localized for
        /// the caller's culture; null for any other kind.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the id the server has logged a fault under; null for a business failure.
        /// </summary>
        public string CorrelationId { get; set; }
    }
}
