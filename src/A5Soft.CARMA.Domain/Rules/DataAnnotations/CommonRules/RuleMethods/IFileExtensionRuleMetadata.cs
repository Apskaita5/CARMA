using A5Soft.CARMA.Domain.Files;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Metadata for <see cref="IFileExtensionRule"/>.
    /// </summary>
    public interface IFileExtensionRuleMetadata : IRuleMetadata
    {
        /// <summary>
        /// Gets a list of allowed file types.
        /// </summary>
        FileType[] AllowedTypes { get; set; }
    }
}
