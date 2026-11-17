using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds an exact (string) length validation rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ExactLengthAttribute : PropertyValidationAttribute, IStringLengthRuleMetadata
    {
        /// <summary>
        ///  exact string length rule
        /// </summary>
        /// <param name="length">string length (that is valid)</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        public ExactLengthAttribute(int length, RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(IExactLengthRule), severity)
        {
            if (length < 1) throw new ArgumentOutOfRangeException(nameof(length),
                length, "Exact length must be a positive number.");
            LengthLimit = length;
            NullIsValid = true;
        }


        /// <inheritdoc cref="IStringLengthRuleMetadata.LengthLimit"/>
        public int LengthLimit { get; }
    }
}
