using System;
using System.Collections.Generic;
using System.Text;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    internal static class RuleEnabledHelper
    {
        /// <summary>
        /// Gets a value indicating whether the rule is enabled for the instance in the context,
        /// i.e. evaluates <see cref="IRuleMetadata.EnabledProperty"/> and <see cref="IRuleMetadata.EnabledInversed"/>.
        /// </summary>
        public static bool IsEnabled(this IValidationContext context)
        {
            var propName = context.RuleMetadata.EnabledProperty;
            if (string.IsNullOrWhiteSpace(propName)) return true;

            var prop = context.EntityMetadata.GetPropertyMetadata(propName.Trim())
                ?? throw new InvalidOperationException($"No metadata for EnabledProperty {propName} " +
                    $"on type {context.EntityMetadata.EntityType.FullName}.");

            if (!(prop.GetValue(context.Instance) is bool enabled))
                throw new InvalidOperationException($"EnabledProperty {propName} " +
                    $"on type {context.EntityMetadata.EntityType.FullName} is not bool.");

            return context.RuleMetadata.EnabledInversed ? !enabled : enabled;
        }
    }
}
