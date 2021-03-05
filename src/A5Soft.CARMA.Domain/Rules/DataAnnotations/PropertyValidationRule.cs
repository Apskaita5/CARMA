using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Default implementation of property validation rules using ValidationAttribute.
    /// </summary>
    public class PropertyValidationRule : IPropertyValidationRule
    {
        private readonly ValidationAttribute _validator;


        public PropertyValidationRule(ValidationAttribute attribute)
        {
            _validator = attribute ?? throw new ArgumentNullException(nameof(attribute));
        }


        /// <inheritdoc cref="IPropertyValidationRule.GetValidationResult" />
        public BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo, 
            IEnumerable<IPropertyMetadata> relatedProps)
        {
            if (instance.IsNull()) throw new ArgumentNullException(nameof(instance));
            if (propInfo.IsNull()) throw new ArgumentNullException(nameof(propInfo));

            if (_validator is IPropertyValidationAttribute attr)
            {
                return attr.GetValidationResult(instance, propInfo, relatedProps);
            }

            var context = new ValidationContext(instance)
            {
                DisplayName = propInfo.GetDisplayName(),
                MemberName = propInfo.Name
            };
            var propValue = propInfo.GetValue(instance);

            var err = _validator.GetValidationResult(propValue, context);
            if (null != err)
            {
                return new BrokenRule(_validator.GetType().FullName, propInfo.Name, 
                    err.ErrorMessage, RuleSeverity.Error);
            }

            return null;
        }

        /// <inheritdoc cref="IPropertyValidationRule.GetRelatedProperties" />
        public List<string> GetRelatedProperties()
        {
            if (_validator is IDependsOnProperties withDependsOnProperties)
                return withDependsOnProperties.DependsOnProperties;
            return new List<string>();
        }

    }
}
