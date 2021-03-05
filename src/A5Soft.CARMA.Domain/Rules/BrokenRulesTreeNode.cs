using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace A5Soft.CARMA.Domain.Rules
{
    /// <summary>
    /// A hierarchical collection of currently broken rules for both an entity and all its child entities.
    /// </summary>
    /// <remarks>
    /// This collection is readonly and can be safely made available
    /// to code outside the business object such as the UI. This allows
    /// external code, such as a UI, to display the list of broken rules
    /// to the user.
    /// </remarks>
    public class BrokenRulesTreeNode
    {
        private readonly List<BrokenRule> _brokenRules;
        private readonly List<BrokenRulesTreeNode> _childEntitiesBrokenRules = new List<BrokenRulesTreeNode>();


        /// <summary>
        /// Creates a new broken rule tree node for an entity.
        /// </summary>
        /// <param name="entityDisplayName">a (localized) name of the entity</param>
        /// <param name="brokenRules">a collection of the broken rules for the entity</param>
        public BrokenRulesTreeNode(string entityDisplayName, BrokenRule[] brokenRules)
        {
            if (null == brokenRules) throw new ArgumentNullException(nameof(brokenRules));
            if (entityDisplayName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(entityDisplayName));

            EntityDisplayName = entityDisplayName;
            _brokenRules = new List<BrokenRule>(brokenRules);
        }


        /// <summary>
        /// Gets a (localized) name of the entity that is described by the node.
        /// </summary>
        public string EntityDisplayName { get; }


        /// <summary>
        /// Adds a child node for a child entity broken rules.
        /// </summary>
        /// <param name="brokenRules">child entity broken rules</param>
        public void AddBrokenRulesForChild(BrokenRulesTreeNode brokenRules)
        {
            if (brokenRules.IsNull()) throw new ArgumentNullException(nameof(brokenRules));
            _childEntitiesBrokenRules.Add(brokenRules);
        }

        /// <summary>
        /// Gets a count of broken rules of the specified severity.
        /// </summary>
        /// <param name="severity">severity of the broken rules to count</param>
        /// <returns>a count of broken rules of the specified severity</returns>
        public int Count(RuleSeverity severity)
        {
            var result = _brokenRules.Count(r => r.Severity == severity);
            foreach (var childEntityBrokenRule in _childEntitiesBrokenRules)
            {
                result += childEntityBrokenRule.Count(severity);
            }
            return result;
        }


        /// <summary>
        /// Gets a description of all broken rules with the severity Error using Environment.NewLine
        /// as a line separator. 
        /// </summary>
        /// <returns>a description of all broken rules with the severity Error using Environment.NewLine
        /// as a line separator</returns>
        public override string ToString()
        {
            return ToString(RuleSeverity.Error, Environment.NewLine);
        }

        /// <summary>
        /// Gets a description of all broken rules with the severity specified
        /// using Environment.NewLine as a line separator.
        /// </summary>
        /// <param name="severity">severity of the broken rules to get a description for</param>
        /// <returns>a description of all broken rules with the severity specified
        /// using Environment.NewLine as a line separator</returns>
        public string ToString(RuleSeverity severity)
        {
            return ToString(severity, Environment.NewLine);
        }

        /// <summary>
        /// Gets a description of all broken rules with the severity Error
        /// using the specified separator as line separator.
        /// </summary>
        /// <param name="separator">a line separator to use</param>
        /// <returns>a description of all broken rules with the severity Error
        /// using the specified separator as line separator</returns>
        public string ToString(string separator)
        {
            return ToString(RuleSeverity.Error, separator);
        }

        /// <summary>
        /// Gets a description of all broken rules with the severity specified
        /// using the specified separator as line separator.
        /// </summary>
        /// <param name="severity">severity of the broken rules to get a description for</param>
        /// <param name="separator">a line separator to use</param>
        /// <returns>a description of all broken rules with the severity specified
        /// using the specified separator as line separator</returns>
        public string ToString(RuleSeverity severity, string separator)
        {
            if (Count(severity) < 1) return string.Empty;

            if (null == separator) separator = string.Empty;

            var result = new StringBuilder();

            foreach (var brokenRule in _brokenRules.Where(r => r.Severity == severity))
            {
                if (result.Length > 0) result.Append(separator);
                result.Append(brokenRule.Description);
            }
            foreach (var childEntityBrokenRule in _childEntitiesBrokenRules)
            {
                childEntityBrokenRule.AddChildDescription(result, severity, separator);
            }

            return result.ToString();
        }


        private void AddChildDescription(StringBuilder builder, RuleSeverity severity, string separator)
        {
            if (Count(severity) < 1) return;

            if (builder.Length > 0) builder.Append(separator);
            builder.Append(EntityDisplayName);
            builder.Append(":");
            foreach (var brokenRule in _brokenRules.Where(r => r.Severity == severity))
            {
                builder.Append(separator);
                builder.Append(brokenRule.Description);
            }
            foreach (var childEntityBrokenRule in _childEntitiesBrokenRules)
            {
                childEntityBrokenRule.AddChildDescription(builder, severity, separator);
            }
        }

    }
}
