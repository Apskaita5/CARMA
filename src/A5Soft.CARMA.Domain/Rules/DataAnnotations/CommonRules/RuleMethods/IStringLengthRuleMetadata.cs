namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Metadata for string length rules, e.g. <see cref="IExactLengthRule"/>.
    /// </summary>
    public interface IStringLengthRuleMetadata : IRuleMetadata
    {
        /// <summary>
        /// Gets a required string length (that is valid).
        /// </summary>
        int LengthLimit { get; }
    }
}
