using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds a min string length validation rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MinLengthAttribute : PropertyValidationAttribute, IStringLengthRuleMetadata
    {
        /// <summary>
        ///  min string length rule
        /// </summary>
        /// <param name="minLength">min string length</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        public MinLengthAttribute(int minLength, RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(IMinLengthRule), severity)
        {
            if (minLength < 1) throw new ArgumentOutOfRangeException(nameof(minLength),
                minLength, "Minimum length must be a positive number (use no rule for 0).");
            LengthLimit = minLength;
            NullIsValid = true;
        }


        /// <inheritdoc cref="IStringLengthRuleMetadata.LengthLimit"/>
        public int LengthLimit { get; }
    }
}
