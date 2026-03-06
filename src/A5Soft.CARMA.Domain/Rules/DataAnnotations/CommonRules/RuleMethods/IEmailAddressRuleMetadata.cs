namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Metadata for <see cref="IEmailAddressRule"/>.
    /// </summary>
    public interface IEmailAddressRuleMetadata : IRuleMetadata
    {
        /// <summary>
        /// Gets a value indicating whether multiple email addresses can be specified (delimited by ;).
        /// </summary>
        public bool AllowMultipleAddresses { get; }
    }
}
