using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds a max string length validation rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MaxLengthAttribute : PropertyValidationAttribute, IStringLengthRuleMetadata
    {
        /// <summary>
        ///  max string length rule
        /// </summary>
        /// <param name="maximumLength">maximum string length</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        public MaxLengthAttribute(int maximumLength, RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(IMaxLengthRule), severity)
        {
            if (maximumLength < 1) throw new ArgumentOutOfRangeException(nameof(maximumLength),
                maximumLength, "Maximum length must be a positive number.");
            LengthLimit = maximumLength;
            NullIsValid = true;
        }


        /// <inheritdoc cref="IStringLengthRuleMetadata.LengthLimit"/>
        public int LengthLimit { get; }
    }
}
