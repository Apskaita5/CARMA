namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Base metadata for numeric value range rules (e.g. min value, max value).
    /// </summary>
    /// <remarks>Rules with extra metadata shall define a descendant interface to accomodate extra fields.</remarks>
    public interface IValueRangePropertyRuleMetadata : IRuleMetadata
    {
        /// <summary>
        /// Gets significant digits when evaluating numeric property value.
        /// Only applicable for numeric properties. Default is 2.
        /// </summary>
        int SignificantDigits { get; }

        /// <summary>
        /// Gets a limiting value (min/max/etc).
        /// </summary>
        public double ValueLimit { get; }
    }
}
