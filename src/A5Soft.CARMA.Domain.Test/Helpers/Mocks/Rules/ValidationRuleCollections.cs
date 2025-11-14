using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules
{
    /// <summary>
    /// Pre-defined validation rule collections for common scenarios.
    /// </summary>
    public static class ValidationRuleCollections
    {
        /// <summary>
        /// Common validation rules for user registration.
        /// </summary>
        public static class UserRegistration
        {
            public static BrokenRule NameRequired =>
                ValidationMockFactory.CreateRequiredFieldError("Name", "Full Name");

            public static BrokenRule EmailRequired =>
                ValidationMockFactory.CreateRequiredFieldError("Email", "Email Address");

            public static BrokenRule EmailInvalidFormat =>
                ValidationMockFactory.CreateEmailFormatError("Email");

            public static BrokenRule PasswordTooShort =>
                ValidationMockFactory.CreateMinLengthError("Password", 8, "Password");

            public static BrokenRule AgeMinimum =>
                ValidationMockFactory.CreateRangeError("Age", 18, 120, "Age");
        }

        /// <summary>
        /// Common validation rules for financial entities.
        /// </summary>
        public static class Financial
        {
            public static BrokenRule AmountRequired =>
                ValidationMockFactory.CreateRequiredFieldError("Amount");

            public static BrokenRule AmountMustBePositive => new BrokenRuleBuilder()
                .WithRuleName("POSITIVE_AMOUNT")
                .ForProperty("Amount")
                .WithDescription("Amount must be greater than zero")
                .AsError()
                .Build();

            public static BrokenRule DateInFuture => new BrokenRuleBuilder()
                .WithRuleName("FUTURE_DATE")
                .ForProperty("Date")
                .WithDescription("Date cannot be in the future")
                .AsError()
                .Build();
        }

        /// <summary>
        /// Common warning rules.
        /// </summary>
        public static class Warnings
        {
            public static BrokenRule LowAge =>
                ValidationMockFactory.CreateWarning(
                    "AGE_LOW",
                    "Age",
                    "Age seems unusually low");

            public static BrokenRule HighAmount =>
                ValidationMockFactory.CreateWarning(
                    "AMOUNT_HIGH",
                    "Amount",
                    "Amount is unusually high");

            public static BrokenRule RecommendVerification =>
                ValidationMockFactory.CreateWarning(
                    "VERIFY_RECOMMENDED",
                    "",
                    "It is recommended to verify this information");
        }
    }
}
