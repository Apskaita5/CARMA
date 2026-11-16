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
    [Serializable]
    public abstract class BrokenRules
    {
        private readonly List<BrokenRule> _brokenRules = new List<BrokenRule>();

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
            var propName = property?.Trim() ?? string.Empty;
            return _brokenRules.FirstOrDefault(c => c.Property == propName && c.Severity == severity);
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
            return Join(separator ?? " ", _ => true);
        }

        /// <summary>
        /// Returns the text of all broken rule descriptions for a specific severity.
        /// </summary>
        /// <param name="separator">String to place between each broken rule description. </param>
        /// <param name="severity">The severity of rules to include in the result.</param>
        /// <returns>The text of all broken rule descriptions matching the specified severtiy.</returns>
        public string ToString(string separator, RuleSeverity severity)
        {
            return Join(separator ?? " ", r => r.Severity == severity);
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
            var propName = propertyName?.Trim() ?? string.Empty;
            return Join(separator ?? " ", r => r.Property == propName && r.Severity == severity);
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

        /// <summary>
        /// Gets a readonly collection of the broken rules.
        /// </summary>
        /// <returns>a readonly collection of the broken rules</returns>
        public IReadOnlyCollection<BrokenRule> ToCollection()
        {
            return _brokenRules.ToArray();
        }


        /// <summary>Adds a broken rule and updates the counts.</summary>
        protected void Add(BrokenRule rule)
        {
            if (null == rule) throw new ArgumentNullException(nameof(rule));
            _brokenRules.Add(rule);
            Recount();
        }

        /// <summary>Adds broken rules and updates the counts.</summary>
        protected void AddRange(IEnumerable<BrokenRule> rules)
        {
            var items = Materialize(rules);
            if (items.Length < 1) return;
            _brokenRules.AddRange(items);
            Recount();
        }

        /// <summary>
        /// Removes all rules matching <paramref name="removeMatch"/>, adds <paramref name="newRules"/>
        /// and updates the counts once.
        /// </summary>
        protected void Replace(Predicate<BrokenRule> removeMatch, IEnumerable<BrokenRule> newRules)
        {
            if (null == removeMatch) throw new ArgumentNullException(nameof(removeMatch));
            var items = Materialize(newRules); // validate before any mutation

            _brokenRules.RemoveAll(removeMatch);
            _brokenRules.AddRange(items);
            Recount();
        }

        /// <summary>Replaces all broken rules and updates the counts.</summary>
        protected void ReplaceAll(IEnumerable<BrokenRule> newRules)
        {
            var items = Materialize(newRules);
            _brokenRules.Clear();
            _brokenRules.AddRange(items);
            Recount();
        }

        /// <summary>Removes all broken rules.</summary>
        protected void Clear()
        {
            _brokenRules.Clear();
            ErrorCount = WarningCount = InformationCount = 0;
        }


        private static BrokenRule[] Materialize(IEnumerable<BrokenRule> rules)
        {
            if (null == rules) return Array.Empty<BrokenRule>();
            var result = rules.ToArray();
            if (result.Any(r => null == r))
                throw new ArgumentException("Broken rules collection cannot contain null items.", nameof(rules));
            return result;
        }

        private void Recount()
        {
            int errors = 0, warnings = 0, infos = 0;
            foreach (var rule in _brokenRules)
            {
                switch (rule.Severity)
                {
                    case RuleSeverity.Error: errors++; break;
                    case RuleSeverity.Warning: warnings++; break;
                    case RuleSeverity.Information: infos++; break;
                    default:
                        throw new InvalidOperationException($"Unhandled severity: {rule.Severity}.");
                }
            }
            // assign only after a successful pass, so an exception doesn't leave partial counts
            ErrorCount = errors;
            WarningCount = warnings;
            InformationCount = infos;
        }

        private string Join(string separator, Func<BrokenRule, bool> filter)
            => string.Join(separator ?? string.Empty, _brokenRules.Where(filter).Select(r => r.Description));
    }
}
