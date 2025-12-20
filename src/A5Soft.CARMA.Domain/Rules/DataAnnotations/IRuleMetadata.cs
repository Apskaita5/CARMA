using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Base metadata extracted from validation attributes.
    /// </summary>
    /// <remarks>Rules with extra metadata shall define a descendant interface to accomodate extra fields.</remarks>
    public interface IRuleMetadata
    {
        /// <summary>
        /// The type of the rule interface.
        /// </summary>
        Type RuleType { get; }

        /// <summary>
        /// Severity of the rule.
        /// </summary>
        RuleSeverity Severity { get; }

        /// <summary>
        /// Gets the list of property names that this rule depends on.
        /// </summary>
        string[] RelatedProperties { get; }
    }
}