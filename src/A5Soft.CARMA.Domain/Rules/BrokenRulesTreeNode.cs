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
    [Serializable]
    public class BrokenRulesTreeNode
    {
        /// <summary>
        /// Default separator between entity names in a child entity path, e.g. "Invoice line 3 > Tax".
        /// </summary>
        public const string DefaultPathSeparator = " > ";

        private readonly List<BrokenRule> _brokenRules;
        private readonly List<BrokenRulesTreeNode> _childEntitiesBrokenRules = new List<BrokenRulesTreeNode>();


        /// <summary>
        /// Creates a new broken rule tree node for an entity.
        /// </summary>
        /// <param name="entityDisplayName">a (localized) name of the entity</param>
        /// <param name="brokenRules">a collection of the broken rules for the entity</param>
        public BrokenRulesTreeNode(string entityDisplayName, IEnumerable<BrokenRule> brokenRules)
        {
            if (null == brokenRules) throw new ArgumentNullException(nameof(brokenRules));

            EntityDisplayName = entityDisplayName ?? string.Empty;
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
            if (null == brokenRules) throw new ArgumentNullException(nameof(brokenRules));
            if (ReferenceEquals(this, brokenRules)) throw new ArgumentException(
                "Cannot add itself.", nameof(brokenRules));
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
            => ToString(severity, separator, DefaultPathSeparator);

        /// <summary>
        /// Gets a description of all broken rules with the severity specified.
        /// Rules of child entities are prefixed by a header containing the path to the child entity.
        /// </summary>
        /// <param name="severity">severity of the broken rules to get a description for</param>
        /// <param name="separator">a line separator to use</param>
        /// <param name="pathSeparator">a separator between entity names in a child entity path</param>
        public string ToString(RuleSeverity severity, string separator, string pathSeparator)
        {
            var result = new StringBuilder();
            AppendDescriptions(result, severity, separator ?? string.Empty,
                pathSeparator ?? string.Empty, null);
            return result.ToString();
        }


        private void AppendDescriptions(StringBuilder builder, RuleSeverity severity,
            string separator, string pathSeparator, string path)
        {
            // root (path == null) and nameless nodes get no header line
            var headerPending = !string.IsNullOrEmpty(path);

            foreach (var brokenRule in _brokenRules.Where(r => r.Severity == severity))
            {
                if (headerPending)
                {
                    AppendLine(builder, separator, path + ":");
                    headerPending = false;
                }
                AppendLine(builder, separator, brokenRule.Description);
            }

            foreach (var child in _childEntitiesBrokenRules)
            {
                child.AppendDescriptions(builder, severity, separator, pathSeparator,
                    CombinePath(path, child.EntityDisplayName, pathSeparator));
            }
        }

        private static string CombinePath(string path, string name, string pathSeparator)
        {
            if (string.IsNullOrEmpty(name)) return path;
            if (string.IsNullOrEmpty(path)) return name;
            return path + pathSeparator + name;
        }

        private static void AppendLine(StringBuilder builder, string separator, string text)
        {
            if (builder.Length > 0) builder.Append(separator);
            builder.Append(text);
        }
    }
}
