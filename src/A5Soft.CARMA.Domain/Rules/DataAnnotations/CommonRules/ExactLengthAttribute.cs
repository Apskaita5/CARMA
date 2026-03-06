using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds an exact (string) length validation rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ExactLengthAttribute : PropertyValidationAttribute, IStringLengthRuleMetadata
    {
        private readonly static Type[] _supportedValueTypes = new Type[] { typeof(string) };


        /// <summary>
        ///  exact string length rule
        /// </summary>
        /// <param name="length">string length (that is valid)</param>
        /// <param name="severity">a value indicating severity of broken rule</param>
        protected ExactLengthAttribute(int length, RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(IExactLengthRule), severity)
        {
            LengthLimit = length;
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
