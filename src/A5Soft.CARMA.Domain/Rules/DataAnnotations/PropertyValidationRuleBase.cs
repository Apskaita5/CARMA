using A5Soft.CARMA.Domain.Metadata;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Base implementation of a property validation rule for rule metadata of type <typeparamref name="TMetadata"/>.
    /// </summary>
    /// <typeparam name="TMetadata">type of the rule metadata interface required</typeparam>
    public abstract class PropertyValidationRuleBase<TMetadata> : IValidationRule
        where TMetadata : IRuleMetadata
    {
        /// <inheritdoc cref="IValidationRule.GetValidationResult(IValidationContext)"/>
        public BrokenRule GetValidationResult(IValidationContext context)
        {
            if (null == context) throw new ArgumentNullException(nameof(context));
            if (null == context.PropertyMetadata) throw new ArgumentException("PropertyMetadata cannot be null.",
                nameof(context));
            if (null == context.RuleMetadata) throw new ArgumentException("RuleMetadata cannot be null.",
                nameof(context));
            if (null == context.Instance) throw new ArgumentException("Instance cannot be null.",
                nameof(context));
            if (null == context.EntityMetadata) throw new ArgumentException("EntityMetadata cannot be null.",
                nameof(context));
            if (!typeof(TMetadata).IsAssignableFrom(context.RuleMetadata.GetType()))
                throw new ArgumentException($"RuleMetadata property ({context.RuleMetadata.GetType().FullName})" +
                    $" is not of type {typeof(TMetadata).FullName}.", nameof(context));

            if (!RuleEnabledHelper.IsEnabled(context)) return null;

            var value = context.PropertyMetadata.GetValue(context.Instance);
            if (value == DBNull.Value) value = null;
            if (null == value)
            {
                if (context.RuleMetadata.NullIsValid) return null;
            }
            else if (!context.PropertyMetadata.PropertyType.IsAssignableFrom(value.GetType()))
            {
                throw new ArgumentException($"Actual property value type ({value.GetType()}) " +
                    $"does not match metadata ({context.PropertyMetadata.PropertyType.FullName}).", nameof(context));
            }

            return GetValidationResult(context.Instance, value, context.EntityMetadata,
                context.PropertyMetadata, (TMetadata)context.RuleMetadata);
        }

        /// <summary>
        /// Only override this method if a custom <see cref="BrokenRule"/> initialization is required.
        /// </summary>
        /// <param name="instance">domain object instance (not null)</param>
        /// <param name="value">a value of the property to validate (could be null)</param>
        /// <param name="entityMetadata">entity metadata for the type of the instance (not null)</param>
        /// <param name="propertyMetadata">metadata for the property the rule is set for (not null)</param>
        /// <param name="ruleMetadata">rule metadata (not null)</param>
        /// <returns>a broken rule if the property value is invalid; null otherwise</returns>
        /// <remarks><see cref="IRuleMetadata.EnabledProperty"/> and <see cref="IRuleMetadata.NullIsValid"/>
        /// are already handled at this point</remarks>
        protected virtual BrokenRule GetValidationResult(object instance, object value,
            IEntityMetadata entityMetadata, IPropertyMetadata propertyMetadata, TMetadata ruleMetadata)
        {
            var description = GetErrorDescription(instance, value, entityMetadata,
                propertyMetadata, ruleMetadata);

            if (string.IsNullOrWhiteSpace(description)) return null;

            return new BrokenRule(this.GetType().FullName, propertyMetadata.Name,
                description, ruleMetadata.Severity);
        }

        /// <summary>
        /// Implement method for actual validation logic for a property.
        /// </summary>
        /// <param name="instance">domain object instance (not null)</param>
        /// <param name="value">a value of the property to validate (could be null)</param>
        /// <param name="entityMetadata">entity metadata for the type of the instance (not null)</param>
        /// <param name="propertyMetadata">metadata for the property the rule is set for (not null)</param>
        /// <param name="ruleMetadata">rule metadata (not null)</param>
        /// <returns>a localized broken rule description if the property value is invalid; null otherwise</returns>
        /// <remarks><see cref="IRuleMetadata.EnabledProperty"/> and <see cref="IRuleMetadata.NullIsValid"/>
        /// are already handled at this point</remarks>
        protected abstract string GetErrorDescription(object instance, object value,
            IEntityMetadata entityMetadata, IPropertyMetadata propertyMetadata, TMetadata ruleMetadata);
    }
}
