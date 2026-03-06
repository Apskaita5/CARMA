using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds an email validation rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class EmailAddressAttribute : PropertyValidationAttribute, IEmailAddressRuleMetadata
    {
        /// <summary>
        /// Email validation rule
        /// </summary>
        public EmailAddressAttribute(RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(IEmailAddressRule), severity)
        {}


        /// <inheritdoc cref="IEmailAddressRuleMetadata.AllowMultipleAddresses"/>
        public bool AllowMultipleAddresses { get; set; } = false;


        protected override void AddRelatedProperties(HashSet<string> relatedProperties)
        {
            base.AddRelatedProperties(relatedProperties);
        }
    }
}
