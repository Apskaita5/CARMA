using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds an url validation rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class UrlAttribute : PropertyValidationAttribute
    {
        /// <summary>
        /// url format rule
        /// </summary>
        public UrlAttribute(RuleSeverity severity = RuleSeverity.Warning)
            : base(typeof(IUrlRule), severity) { }
    }
}
