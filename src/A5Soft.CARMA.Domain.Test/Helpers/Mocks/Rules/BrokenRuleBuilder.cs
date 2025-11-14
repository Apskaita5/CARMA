using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules
{
    /// <summary>
    /// Fluent builder for creating BrokenRule instances.
    /// </summary>
    public class BrokenRuleBuilder
    {
        private string _ruleName = "RULE";
        private string _property = string.Empty;
        private string _description = "Validation failed";
        private RuleSeverity _severity = RuleSeverity.Error;

        public BrokenRuleBuilder WithRuleName(string ruleName)
        {
            _ruleName = ruleName;
            return this;
        }

        public BrokenRuleBuilder ForProperty(string propertyName)
        {
            _property = propertyName;
            return this;
        }

        public BrokenRuleBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public BrokenRuleBuilder WithSeverity(RuleSeverity severity)
        {
            _severity = severity;
            return this;
        }

        public BrokenRuleBuilder AsError()
        {
            _severity = RuleSeverity.Error;
            return this;
        }

        public BrokenRuleBuilder AsWarning()
        {
            _severity = RuleSeverity.Warning;
            return this;
        }

        public BrokenRuleBuilder AsInfo()
        {
            _severity = RuleSeverity.Information;
            return this;
        }

        public BrokenRule Build()
        {
            return new BrokenRule(_ruleName, _property, _description, _severity);
        }

        public static implicit operator BrokenRule(BrokenRuleBuilder builder)
            => builder.Build();
    }
}
