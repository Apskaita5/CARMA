using A5Soft.CARMA.Domain.Rules;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules
{
    /// <summary>
    /// Helper methods for validation testing.
    /// </summary>
    public static class ValidationTestHelpers
    {
        /// <summary>
        /// Creates a validation provider that fails with specific errors for testing.
        /// </summary>
        public static IValidationEngineProvider CreateFailingProvider<TEntity>(
            params BrokenRule[] rules)
        {
            var engine = new ValidationEngineMockBuilder(typeof(TEntity));

            foreach (var rule in rules)
            {
                engine.WithPropertyRule(rule.Property, rule);
            }

            return new ValidationEngineProviderMockBuilder()
                .WithEngine(typeof(TEntity), engine.Build())
                .Build();
        }

        /// <summary>
        /// Creates a simple broken rule for testing.
        /// </summary>
        public static BrokenRule CreateError(string propertyName, string message)
        {
            return new BrokenRuleBuilder()
                .ForProperty(propertyName)
                .WithDescription(message)
                .AsError()
                .Build();
        }

        /// <summary>
        /// Asserts that a property has a specific validation error.
        /// </summary>
        public static bool HasError(
            this IValidationEngine engine,
            object instance,
            string propertyName,
            string errorCode)
        {
            var rules = engine.GetBrokenRules(instance, propertyName);
            return rules.Any(r => r.RuleName == errorCode);
        }

        /// <summary>
        /// Gets all error messages for a property.
        /// </summary>
        public static IEnumerable<string> GetErrorMessages(
            this IValidationEngine engine,
            object instance,
            string propertyName)
        {
            return engine.GetBrokenRules(instance, propertyName)
                .Where(r => r.Severity == RuleSeverity.Error)
                .Select(r => r.Description);
        }
    }
}
