namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Metadata for <see cref="IAtLeastOneRequiredRule"/>.
    /// </summary>
    public interface IAtLeastOneRequiredRuleMetadata : IRefPropertyRuleMetadata
    {
        /// <summary>
        /// Gets a value indicating whether the refernce property value is a reference (primary key) of a domain entity.
        /// </summary>
        bool IsKeyReference { get; }

        /// <summary>
        /// Gets a value indicating whether negative numeric property value is valid.
        /// Only applicable for numeric properties. Default is false.
        /// </summary>
        bool AllowNegative { get; }

        /// <summary>
        /// Gets significant digits when evaluating numeric property value.
        /// Only applicable for numeric properties. Default is 2.
        /// </summary>
        int SignificantDigits { get; }
    }
}
