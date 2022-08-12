using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for an entity rule, i.e. a rule that is designed for
    /// a specific domain entity, not a particular property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public abstract class EntityRuleAttributeBase : ValidationAttribute,
        IPropertyValidationAttribute
    {
        /// <inheritdoc />
        protected EntityRuleAttributeBase() { }


        /// <summary>
        /// Whether the rule should be evaluated by a generic data annotations engine
        /// (that is not aware of rule severity)
        /// </summary>
        protected virtual bool VisibleToGenericEngine { get; } = true;

        /// <inheritdoc />
        public override bool RequiresValidationContext
            => true;


        /// <inheritdoc cref="IPropertyValidationAttribute.GetValidationResult" />
        public BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo,
            IEnumerable<IPropertyMetadata> relatedProps)
        {
            return GetValidationResult(instance);
        }


        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (null == validationContext) throw new ArgumentNullException(nameof(validationContext));

            if (!VisibleToGenericEngine) return null;

            var result = GetValidationResult(validationContext.ObjectInstance);
            if (null != result && result.Severity == RuleSeverity.Error)
                return new ValidationResult(result.Description);

            return null;
        }

        /// <summary>
        /// Implement the method to do actual validation.
        /// </summary>
        /// <param name="instance">Domain object instance to validate.</param>
        protected abstract BrokenRule GetValidationResult(object instance);
    }
}
