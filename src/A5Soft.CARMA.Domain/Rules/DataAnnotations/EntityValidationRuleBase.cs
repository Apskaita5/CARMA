using A5Soft.CARMA.Domain.Metadata;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Base implementation of a type <typeparamref name="TInstance"/> entity validation rule for rule metadata of tyoe <typeparamref name="TMetadata"/>.
    /// </summary>
    /// <typeparam name="TMetadata">type of rule metadata interface required</typeparam>
    /// <typeparam name="TInstance">type of the instances that the rule is applicable for</typeparam>
    public abstract class EntityValidationRuleBase<TMetadata, TInstance> : IValidationRule
        where TMetadata : IRuleMetadata
    {
        /// <inheritdoc cref="IValidationRule.GetValidationResult(IValidationContext)"/>
        public BrokenRule GetValidationResult(IValidationContext context)
        {
            if (null == context) throw new ArgumentNullException(nameof(context));
            if (!typeof(TMetadata).IsAssignableFrom(context.RuleMetadata.GetType()))
                throw new ArgumentException($"RuleMetadata property ({context.RuleMetadata.GetType().FullName})" +
                    $" is not of type {typeof(TMetadata).FullName}.", nameof(context));
            if (!typeof(TInstance).IsAssignableFrom(context.Instance.GetType()))
                throw new ArgumentException($"Entity instance ({context.Instance.GetType().FullName})" +
                    $" is not of type {typeof(TInstance).FullName}.", nameof(context));

            return GetValidationResult((TInstance)context.Instance, context.EntityMetadata,
                (TMetadata)context.RuleMetadata);
        }

        /// <summary>
        /// Implement method for actual validation logic for a property.
        /// </summary>
        /// <param name="instance">entity instance (not null)</param>
        /// <param name="entityMetadata">entity metadata for the type <typeparamref name="TInstance"/> (not null)</param>
        /// <param name="ruleMetadata">rule metadata (not null)</param>
        /// <returns>a broken rule if the entity state is invalid; null otherwise</returns>
        protected abstract BrokenRule GetValidationResult(TInstance instance, IEntityMetadata entityMetadata,
            TMetadata ruleMetadata);
    }
}
