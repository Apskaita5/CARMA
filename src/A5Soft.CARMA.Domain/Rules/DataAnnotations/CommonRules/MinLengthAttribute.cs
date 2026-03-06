using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds a min string length validation rule.
    /// </summary>
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
            LengthLimit = minLength;
            NullIsValid = true;
        }


        /// <inheritdoc cref="IStringLengthRuleMetadata.LengthLimit"/>
        public int LengthLimit { get; }


        protected override void AddRelatedProperties(HashSet<string> relatedProperties)
        {
            base.AddRelatedProperties(relatedProperties);
        }
    }
}
