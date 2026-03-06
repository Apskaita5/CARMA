using A5Soft.CARMA.Domain.Metadata;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Base implementation of a custom property validation rule for an entity of type <typeparamref name="TInstance"/>
    /// and rule metadata of type <typeparamref name="TMetadata"/>.
    /// Custom rules are defined for specific entities ad hoc.
    /// </summary>
    /// <typeparam name="TMetadata">type of the rule metadata interface required</typeparam>
    /// <typeparam name="TInstance">type of the entity instance</typeparam>
    public abstract class CustomPropertyValidationRuleBase<TMetadata, TInstance> : IValidationRule
        where TMetadata : IRuleMetadata
    {
        /// <inheritdoc cref="IValidationRule.GetValidationResult(IValidationContext)"/>
        public BrokenRule GetValidationResult(IValidationContext context)
        {
            if (null == context) throw new ArgumentNullException(nameof(context));
            if (null == context.PropertyMetadata) throw new ArgumentException("PropertyMetadata cannot be null.",
                nameof(context));
            if (!typeof(TMetadata).IsAssignableFrom(context.RuleMetadata.GetType()))
                throw new ArgumentException($"RuleMetadata property ({context.RuleMetadata.GetType().FullName})" +
                    $" is not of type {typeof(TMetadata).FullName}.", nameof(context));
            if (!typeof(TInstance).IsAssignableFrom(context.Instance.GetType()))
                throw new ArgumentException($"Entity instance ({context.Instance.GetType().FullName})" +
                    $" is not of type {typeof(TInstance).FullName}.", nameof(context));

            return GetValidationResult((TInstance)context.Instance, context.EntityMetadata, context.PropertyMetadata,
                (TMetadata)context.RuleMetadata);
        }

        /// <summary>
        /// Only override this method if a custom <see cref="BrokenRule"/> initialization is required.
        /// </summary>
        /// <param name="instance">entity instance (not null)</param>
        /// <param name="entityMetadata">entity metadata for the type of the instance (not null)</param>
        /// <param name="propertyMetadata">metadata for the property the rule is set for (not null)</param>
        /// <param name="ruleMetadata">rule metadata (not null)</param>
        /// <returns>a broken rule if the property value is invalid; null otherwise</returns>
        protected virtual BrokenRule GetValidationResult(TInstance instance, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, TMetadata ruleMetadata)
        {
            var description = GetErrorDescription(instance, entityMetadata, propertyMetadata, ruleMetadata);

            if (string.IsNullOrWhiteSpace(description)) return null;

            return new BrokenRule(this.GetType().FullName, propertyMetadata.Name,
                description, ruleMetadata.Severity);
        }

        /// <summary>
        /// Implement method for actual validation logic for a property.
        /// </summary>
        /// <param name="instance">domain object instance (not null)</param>
        /// <param name="entityMetadata">entity metadata for the type of the instance (not null)</param>
        /// <param name="propertyMetadata">metadata for the property the rule is set for (not null)</param>
        /// <param name="ruleMetadata">rule metadata (not null)</param>
        /// <returns>a localized broken rule description if the property value is invalid; null otherwise</returns>
        /// <remarks><see cref="IRuleMetadata.EnabledProperty"/> and <see cref="IRuleMetadata.NullIsValid"/>
        /// are NOT handled at this point (because property value access is expensive)</remarks>
        protected abstract string GetErrorDescription(TInstance instance, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, TMetadata ruleMetadata);
    }
}
