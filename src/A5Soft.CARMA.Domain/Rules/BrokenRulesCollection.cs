using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules
{
    /// <summary>
    /// Encapsulates a list of <see cref="BrokenRule"/> as a result of POCO or custom validation.
    /// See <see cref="Extensions.ValidatePoco{T}"/>.
    /// </summary>
    public class BrokenRulesCollection : BrokenRules
    {
        /// <inheritdoc />
        public BrokenRulesCollection(List<BrokenRule> brokenRules) : base()
        {
            if (null != brokenRules)
            {
                _brokenRules.AddRange(brokenRules);
                ResetCount();
            }
        }

        /// <inheritdoc />
        public BrokenRulesCollection(BrokenRule brokenRule) : base()
        {
            if (null == brokenRule) throw new ArgumentNullException(nameof(brokenRule));

            _brokenRules.Add(brokenRule);
            ResetCount();
        }

        /// <inheritdoc />
        public BrokenRulesCollection(string description, string property = null,
            RuleSeverity severity = RuleSeverity.Error) : base()
        {
            if (description.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(description));

            _brokenRules.Add(new BrokenRule(string.Empty, property, description, severity));
            ResetCount();
        }

        /// <inheritdoc />
        public BrokenRulesCollection() : base() { }


        /// <summary>
        /// Adds a custom broken rule to the collection.
        /// </summary>
        /// <param name="property">property affected by the rule (if any)</param>
        /// <param name="description">description of the broken rule (localized error message, required)</param>
        /// <param name="severity">severity of the broken rule</param>
        public void AddCustomBrokenRule(string description, string property = null, 
            RuleSeverity severity = RuleSeverity.Error)
        {
            if (description.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(description));

            _brokenRules.Add(new BrokenRule(string.Empty, property, description, severity));
            ResetCount();
        }
    }
}
