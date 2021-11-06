using A5Soft.CARMA.Domain.Metadata;
using System;
using System.ComponentModel.DataAnnotations;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Default implementation of entity validation rule using validation attributes.
    /// </summary>
    public class EntityValidationRule : IEntityValidationRule
    {
        private readonly ValidationAttribute _validator;


        public EntityValidationRule(ValidationAttribute attribute)
        {
            _validator = attribute ?? throw new ArgumentNullException(nameof(attribute));
        }


        /// <inheritdoc cref="IEntityValidationRule.GetValidationResult" />
        public BrokenRule GetValidationResult(object instance, IEntityMetadata entityInfo)
        {
            if (instance.IsNull()) throw new ArgumentNullException(nameof(instance));
            if (entityInfo.IsNull()) throw new ArgumentNullException(nameof(entityInfo));

            if (_validator is IEntityValidationAttribute attr)
            {
                return attr.GetValidationResult(instance, entityInfo);
            }

            // because if it could be validated yet has no IsNew prop,
            // than it's a singleton domain object which is never new 
            var isNew = (instance as IPersisted)?.IsNew ?? false;

            var context = new ValidationContext(instance)
            {
                DisplayName = isNew ? entityInfo.GetDisplayNameForNew() 
                    : entityInfo.GetDisplayNameForOld()
            };

            var err = _validator.GetValidationResult(instance, context);
            if (null != err)
            {
                return new BrokenRule(_validator.GetType().FullName, string.Empty, 
                    err.ErrorMessage, RuleSeverity.Error);
            }

            return null;
        }

    }
}
