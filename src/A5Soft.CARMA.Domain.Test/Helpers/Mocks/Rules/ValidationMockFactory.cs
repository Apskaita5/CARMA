using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;
using System;

namespace A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules
{
    /// <summary>
    /// Factory for creating common validation mocks.
    /// </summary>
    public static class ValidationMockFactory
    {
        /// <summary>
        /// Creates a BrokenRule for a required field validation.
        /// </summary>
        public static BrokenRule CreateRequiredFieldError(
            string propertyName,
            string displayName = null)
        {
            return new BrokenRuleBuilder()
                .WithRuleName("REQUIRED")
                .ForProperty(propertyName)
                .WithDescription($"{displayName ?? propertyName} is required")
                .AsError()
                .Build();
        }

        /// <summary>
        /// Creates a BrokenRule for minimum length validation.
        /// </summary>
        public static BrokenRule CreateMinLengthError(
            string propertyName,
            int minLength,
            string displayName = null)
        {
            return new BrokenRuleBuilder()
                .WithRuleName("MIN_LENGTH")
                .ForProperty(propertyName)
                .WithDescription($"{displayName ?? propertyName} must be at least {minLength} characters")
                .AsError()
                .Build();
        }

        /// <summary>
        /// Creates a BrokenRule for maximum length validation.
        /// </summary>
        public static BrokenRule CreateMaxLengthError(
            string propertyName,
            int maxLength,
            string displayName = null)
        {
            return new BrokenRuleBuilder()
                .WithRuleName("MAX_LENGTH")
                .ForProperty(propertyName)
                .WithDescription($"{displayName ?? propertyName} must not exceed {maxLength} characters")
                .AsError()
                .Build();
        }

        /// <summary>
        /// Creates a BrokenRule for email format validation.
        /// </summary>
        public static BrokenRule CreateEmailFormatError(string propertyName)
        {
            return new BrokenRuleBuilder()
                .WithRuleName("EMAIL_FORMAT")
                .ForProperty(propertyName)
                .WithDescription("Email address is not in a valid format")
                .AsError()
                .Build();
        }

        /// <summary>
        /// Creates a BrokenRule for range validation.
        /// </summary>
        public static BrokenRule CreateRangeError(
            string propertyName,
            object minValue,
            object maxValue,
            string displayName = null)
        {
            return new BrokenRuleBuilder()
                .WithRuleName("RANGE")
                .ForProperty(propertyName)
                .WithDescription($"{displayName ?? propertyName} must be between {minValue} and {maxValue}")
                .AsError()
                .Build();
        }

        /// <summary>
        /// Creates a warning rule.
        /// </summary>
        public static BrokenRule CreateWarning(
            string ruleName,
            string propertyName,
            string message)
        {
            return new BrokenRuleBuilder()
                .WithRuleName(ruleName)
                .ForProperty(propertyName)
                .WithDescription(message)
                .AsWarning()
                .Build();
        }

        /// <summary>
        /// Creates a validation engine provider that always validates successfully.
        /// </summary>
        public static IValidationEngineProvider CreatePassingProvider(params Type[] forTypes)
        {
            var builder = new ValidationEngineProviderMockBuilder();
            foreach (var t in forTypes)
            {
                builder.WithAllValid(t);
            }
            return builder.Build();
        }

        /// <summary>
        /// Creates a validation engine provider with no engines registered.
        /// </summary>
        public static IValidationEngineProvider CreateEmptyProvider()
        {
            return new ValidationEngineProviderMockBuilder()
                .Build();
        }
    }
}
