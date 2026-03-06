using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Base metadata for numeric (incl DateTime) rules that references another numeric (incl DateTime) property
    /// (e.g. greater than).
    /// </summary>
    /// <remarks>Rules with extra metadata shall define a descendant interface to accomodate extra fields.</remarks>
    public interface INumericRefPropertyRuleMetadata : IRefPropertyRuleMetadata
    {
        /// <summary>
        /// Gets significant digits when evaluating numeric property value.
        /// Only applicable for numeric properties. Default is 2.
        /// </summary>
        public int SignificantDigits { get; }

        /// <summary>
        /// Gets a value indicating whether the time component of a <see cref="DateTime"/>
        /// values should be evaluated.
        /// Only applicable for <see cref="DateTime"/> properties. Default is false.
        /// </summary>
        public bool EvaluateTime { get; }

        /// <summary>
        /// Gets a value indicating whether equal property values are valid as well.
        /// </summary>
        public bool EqualValuesAreValid { get; }
    }
}
