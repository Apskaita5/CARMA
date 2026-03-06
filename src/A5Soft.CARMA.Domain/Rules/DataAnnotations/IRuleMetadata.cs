using System;
using System.Collections.Generic;

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
        /// Whether a null value is always considered as valid.
        /// </summary>
        bool NullIsValid { get; }

        /// <summary>
        /// A name of a bool property that determines whether the rule is checked. (if any)
        /// </summary>
        /// <remarks>the property should be of type <see cref="bool"/></remarks>
        string EnabledProperty { get; }

        /// <summary>
        /// Whether <see cref="EnabledProperty"/> value needs to be inversed,
        /// i.e. rule is only checked when <see cref="EnabledProperty"/> is false.
        /// </summary>
        bool EnabledInversed { get; }


        /// <summary>
        /// Gets the list of property names that this rule depends on.
        /// </summary>
        HashSet<string> GetRelatedProperties();
    }
}