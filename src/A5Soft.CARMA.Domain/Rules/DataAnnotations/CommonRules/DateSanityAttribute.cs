using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds date sanity rule, i.e. not in future and not too far in past (20 years or more).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DateSanityAttribute : PropertyValidationAttribute
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
    }
}
