using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Domain.Rules
{
    /// <summary>
    /// Encapsulates a list of <see cref="BrokenRule"/> as a result of POCO or custom validation.
    /// See <see cref="Extensions.ValidatePoco{T}"/>.
    /// </summary>
    [Serializable]
    public class BrokenRulesCollection : BrokenRules
    {
        /// <inheritdoc />
        public BrokenRulesCollection(IEnumerable<BrokenRule> brokenRules) : base()
        {
            if (null != brokenRules)
            {
                AddRange(brokenRules.Where(r => null != r));
            }
        }

        /// <inheritdoc />
        public BrokenRulesCollection(BrokenRule brokenRule) : base()
        {
            if (null == brokenRule) throw new ArgumentNullException(nameof(brokenRule));

            Add(brokenRule);
        }

        /// <inheritdoc />
        public BrokenRulesCollection(string description, string property = null,
            RuleSeverity severity = RuleSeverity.Error) : base()
        {
            if (description.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(description));

            Add(new BrokenRule(string.Empty, property, description, severity));
        }

        /// <inheritdoc />
        public BrokenRulesCollection() : base() { }
    }
}
