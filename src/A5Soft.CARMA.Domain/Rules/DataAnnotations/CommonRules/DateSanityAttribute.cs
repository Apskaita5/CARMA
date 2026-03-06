using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds date sanity rule, i.e. not in future and not too far in past (20 years or more).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DateSanityAttribute : PropertyValidationAttribute, IRuleMetadata
    {
        /// <summary>
        /// Date sanity rule, i.e. not in future and not too far in past (20 years or more).
        /// </summary>
        /// <param name="severity">Rule severity, default - <see cref="RuleSeverity.Warning"/>.</param>
        public DateSanityAttribute(RuleSeverity severity = RuleSeverity.Warning)
            : base(typeof(IDateSanityRule), severity)
        {
            NullIsValid = true;
        }


        protected override void AddRelatedProperties(HashSet<string> relatedProperties)
        {
            base.AddRelatedProperties(relatedProperties);
        }
    }
}
