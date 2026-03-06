namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Metadata for <see cref="IRequiredRule"/>.
    /// </summary>
    public interface IRequiredRuleMetadata : IRuleMetadata
    {
        /// <summary>
        /// Gets or sets a value indicating whether the property value is a reference (primary key) to a domain entity.
        /// </summary>
        bool IsKeyReference { get; }

        /// <summary>
        /// Gets or sets a value indicating whether an empty string is valid.
        /// Only applicable for string properties. Default is false.
        /// </summary>
        bool AllowEmptyStrings { get; }

        /// <summary>
        /// Gets or sets a value indicating whether negative numeric property value is valid.
        /// Only applicable for numeric properties. Default is false.
        /// </summary>
        bool AllowNegative { get; }

        /// <summary>
        /// Gets or sets significant digits when evaluating numeric property value.
        /// Only applicable for numeric properties. Default is 2.
        /// </summary>
        int SignificantDigits { get; }
    }
}
