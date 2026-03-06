using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds a max value validation rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MaxValueAttribute : PropertyValidationAttribute, IValueRangePropertyRuleMetadata
    {
        /// <summary>
        /// max decimal (double, float) value validation
        /// </summary>
        /// <param name="maxValue">max allowed value</param>
        /// <param name="digits">significant digits to compare double or decimal values (default - 2)</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        public MaxValueAttribute(double maxValue, int digits = 2, RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(IMaxValueRule), severity)
        {
            SignificantDigits = digits;
            DblValueLimit = maxValue;
            ExpectedIntValue = false;
            NullIsValid = true;
        }

        /// <summary>
        /// max int value validation
        /// </summary>
        /// <param name="maxValue">max allowed value</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        public MaxValueAttribute(int maxValue, RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(IMaxValueRule), severity)
        {
            SignificantDigits = 2;
            IntValueLimit = maxValue;
            ExpectedIntValue = true;
            NullIsValid = true;
        }


        /// <inheritdoc cref="IValueRangePropertyRuleMetadata.ExpectedIntValue"/>
        public bool ExpectedIntValue { get; }

        /// <inheritdoc cref="IValueRangePropertyRuleMetadata.IntValueLimit"/>
        public int IntValueLimit { get; }

        /// <inheritdoc cref="IValueRangePropertyRuleMetadata.DblValueLimit"/>
        public double DblValueLimit { get; }

        /// <inheritdoc cref="IValueRangePropertyRuleMetadata.SignificantDigits"/>
        public int SignificantDigits { get; }


        protected override void AddRelatedProperties(HashSet<string> relatedProperties)
        {
            base.AddRelatedProperties(relatedProperties);
        }
    }
}
