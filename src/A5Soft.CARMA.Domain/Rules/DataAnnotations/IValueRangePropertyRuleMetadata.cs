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
        /// Gets a value indicating wheteher an integer value is expected (otherwise decimal, double or float).
        /// </summary>
        public bool ExpectedIntValue { get; }

        /// <summary>
        /// Gets a limiting integer value (min/max/etc).
        /// </summary>
        public int IntValueLimit { get; }

        /// <summary>
        /// Gets a limiting double (decimal, float) value (min/max/etc).
        /// </summary>
        public double DblValueLimit { get; }
    }
}
