using System;

namespace A5Soft.CARMA.Domain.Rules
{
    /// <summary>
    /// Stores details about a specific broken business rule.
    /// </summary>
    [Serializable]
    public class BrokenRule
    {

        public BrokenRule(string ruleName, string property, string description, RuleSeverity severity)
        {
            if (description.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(description));

            RuleName = ruleName ?? string.Empty;
            Property = property ?? string.Empty;
            Description = description;
            Severity = severity;
        }


        /// <summary>
        /// Provides access to the name of the broken rule.
        /// </summary>
        /// <value>The name of the rule.</value>
        public string RuleName { get; }

        /// <summary>
        /// Provides access to the description of the broken rule.
        /// </summary>
        /// <value>The description of the rule.</value>
        public string Description { get; }

        /// <summary>
    /// Provides access to the property affected by the broken rule.
    /// </summary>
    /// <value>The property affected by the rule.</value>
        public string Property { get; }

        /// <summary>
        /// Gets the severity of the broken rule.
        /// </summary>
        public RuleSeverity Severity { get; }


        /// <summary>
        /// Gets a string representation for this object.
        /// </summary>
        public override string ToString()
        {
            return Description;
        }

    }
}
