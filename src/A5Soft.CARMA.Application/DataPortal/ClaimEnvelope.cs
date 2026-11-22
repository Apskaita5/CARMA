namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A claim as it crosses the data portal.
    /// </summary>
    public sealed class ClaimEnvelope
    {
        /// <summary>Gets or sets a claim type.</summary>
        public string Type { get; set; }

        /// <summary>Gets or sets a claim value.</summary>
        public string Value { get; set; }
    }
}
