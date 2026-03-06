using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Metadata for <see cref="IEqualToRule"/>.
    /// </summary>
    public interface IEqualToRuleMetadata : IRefPropertyRuleMetadata
    {
        /// <summary>
        /// Gets a string comparision method (if comparing string values).
        /// </summary>
        StringComparison StringComparison { get; }

        /// <summary>
        /// Gets significant digits when evaluating numeric property value.
        /// Only applicable for numeric properties. Default is 2.
        /// </summary>
        int SignificantDigits { get; }
    }
}
