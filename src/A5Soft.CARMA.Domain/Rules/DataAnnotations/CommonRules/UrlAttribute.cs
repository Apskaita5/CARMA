using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds an url validation rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class UrlAttribute : PropertyValidationAttribute, IRuleMetadata
    {
        /// <summary>
        /// url format rule
        /// </summary>
        public UrlAttribute(RuleSeverity severity = RuleSeverity.Warning)
            : base(typeof(IUrlRule), severity) { }


        protected override void AddRelatedProperties(HashSet<string> relatedProperties)
        {
            base.AddRelatedProperties(relatedProperties);
        }
    }
}
