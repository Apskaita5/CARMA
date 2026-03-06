namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Metadata for <see cref="IMaxFileSizeRule"/>.
    /// </summary>
    public interface IMaxFileSizeRuleMetadata : IRuleMetadata
    {
        /// <summary>
        /// Gets a max allowed file size (MB).
        /// </summary>
        int MaxSizeMB { get; }
    }
}
