using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace A5Soft.CARMA.Domain.Rules
{
    /// <summary>
    /// A collection of currently broken rules.
    /// </summary>
    /// <remarks>
    /// This collection is readonly and can be safely made available
    /// to code outside the business object such as the UI. This allows
    /// external code, such as a UI, to display the list of broken rules
    /// to the user.
    /// </remarks>
    public abstract class BrokenRules
    {
        protected readonly List<BrokenRule> _brokenRules = new List<BrokenRule>();

        protected BrokenRules() { }


        /// <summary>
        /// Gets the number of broken rules in the collection that have a severity of Error.
        /// </summary>
        /// <value>An integer value.</value>
        public int ErrorCount { get; private set; }

        /// <summary>
        /// Gets the number of broken rules in the collection that have a severity of Warning.
        /// </summary>
        /// <value>An integer value.</value>
        public int WarningCount { get; private set; }

        /// <summary>
        /// Gets the number of broken rules in the collection that have a severity of Information.
        /// </summary>
        /// <value>An integer value.</value>
        public int InformationCount { get; private set; }



        /// <summary>
        /// Returns the first <see cref="BrokenRule"/> object corresponding to the specified property
        /// and severity.
        /// </summary>
        /// <param name="property">The name of the property affected by the rule.</param>
        /// <param name="severity">The severity of broken rule to return.</param>
        /// <returns>
        /// The first BrokenRule object corresponding to the specified property, or null
        /// if there are no rules defined for the property.
        /// </returns>
        public BrokenRule GetFirstBrokenRule(string property, RuleSeverity severity = RuleSeverity.Error)
        {
            return _brokenRules.FirstOrDefault(c => c.Property == property && c.Severity == severity);
        }


        /// <summary>
        /// Returns the text of all broken rule descriptions, each separated by a <see cref="Environment.NewLine" />.
        /// </summary>
        /// <returns>The text of all broken rule descriptions.</returns>
        public override string ToString()
        {
            return ToString(Environment.NewLine);
        }

        /// <summary>
        /// Returns the text of all broken rule descriptions for a specific severity, each
        /// separated by a <see cref="Environment.NewLine" />.
        /// </summary>
        /// <param name="severity">The severity of rules to  include in the result.</param>
        /// <returns>The text of all broken rule descriptions matching the specified severtiy.</returns>
        public string ToString(RuleSeverity severity)
        {
            return ToString(Environment.NewLine, severity);
        }

        /// <summary>
        /// Returns the text of all broken rule descriptions.
        /// </summary>
        /// <param name="separator">String to place between each broken rule description.
        /// </param>
        /// <returns>The text of all broken rule descriptions.</returns>
        public string ToString(string separator)
        {
            StringBuilder result = new StringBuilder();
            foreach (BrokenRule item in _brokenRules)
            {
                if (result.Length > 0) result.Append(separator);
                result.Append(item.Description);
            }
            return result.ToString();
        }

        /// <summary>
        /// Returns the text of all broken rule descriptions for a specific severity.
        /// </summary>
        /// <param name="separator">String to place between each broken rule description. </param>
        /// <param name="severity">The severity of rules to include in the result.</param>
        /// <returns>The text of all broken rule descriptions matching the specified severtiy.</returns>
        public string ToString(string separator, RuleSeverity severity)
        {
            StringBuilder result = new StringBuilder();
            foreach (BrokenRule item in _brokenRules.Where(r => r.Severity == severity))
            {
                if (result.Length > 0) result.Append(separator);
                result.Append(item.Description);
            }
            return result.ToString();
        }

        /// <summary>
        /// Returns the text of all broken rule descriptions for a specific severity and property.
        /// </summary>
        /// <param name="separator"> String to place between each broken rule description. </param>
        /// <param name="severity">The severity of rules to include in the result.</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>The text of all broken rule descriptions  matching the specified severtiy.</returns>
        public string ToString(string separator, RuleSeverity severity, string propertyName)
        {
            StringBuilder result = new StringBuilder();
            foreach (BrokenRule item in _brokenRules
                .Where(r => r.Property == propertyName && r.Severity == severity))
            {
                if (result.Length > 0) result.Append(separator);
                result.Append(item.Description);
            }
            return result.ToString();
        }

        /// <summary>
        /// Returns a string array containing all broken rule descriptions.
        /// </summary>
        /// <returns>The text of all broken rule descriptions.</returns>
        public string[] ToArray()
        {
            return _brokenRules.Select(c => c.Description).ToArray();
        }

        /// <summary>
        /// Returns a string array containing all broken rule descriptions matching the specified severity.
        /// </summary>
        /// <param name="severity">The severity of rules to include in the result.</param>
        /// <returns>The text of all broken rule descriptions matching the specified severity.</returns>
        public string[] ToArray(RuleSeverity severity)
        {
            return _brokenRules.Where(c => c.Severity == severity).Select(c => c.Description).ToArray();
        }

        internal BrokenRule[] ToBrokenRuleArray()
            => _brokenRules.ToArray();


        protected void ResetCount()
        {
            ErrorCount = WarningCount = InformationCount = 0;
            foreach (var brokenRule in _brokenRules)
            {
                switch (brokenRule.Severity)
                {
                    case RuleSeverity.Error:
                        ErrorCount += 1;
                        break;
                    case RuleSeverity.Warning:
                        WarningCount += 1;
                        break;
                    case RuleSeverity.Information:
                        InformationCount += 1;
                        break;
                    default:
                        throw new NotImplementedException("unhandled severity=" + brokenRule.Severity);
                }
            }
        }

    }
}
