using System.Collections.Generic;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Rules
{
    /// <summary>
    /// Common interface for validation engines.
    /// </summary>
    public interface IValidationEngine
    {

        /// <summary>
        /// Gets a metadata for the entity that is the target of the validation engine.
        /// </summary>
        IEntityMetadata EntityMetadata { get; }


        /// <summary>
        /// Checks business rules for the property specified (excluding dependent properties)
        /// and returns a list of broken rules. (empty list, if no broken rules)
        /// </summary>
        /// <param name="instance">an instance of the entity to check the rules for</param>
        /// <param name="propName">a property (name) to check the rules for</param>
        /// <returns>a list of broken rules (empty list, if no broken rules)</returns>
        List<BrokenRule> GetBrokenRules(object instance, string propName);

        /// <summary>
        /// Checks business rules for the property specified and returns a list of broken rules.
        /// (empty list, if no broken rules)
        /// </summary>
        /// <param name="instance">an instance of the entity to check the rules for</param>
        /// <param name="propName">a property (name) to check the rules for</param>
        /// <param name="includeDependentProps">whether to check dependent properties as well</param>
        /// <returns>a list of broken rules (empty list, if no broken rules)</returns>
        List<BrokenRule> GetBrokenRules(object instance, string propName, bool includeDependentProps);

        /// <summary>
        /// Checks entity level business rules and returns a list of broken rules.
        /// (empty list, if no broken rules)
        /// </summary>
        /// <param name="instance">an instance of the entity to check the rules for</param>
        /// <returns>a list of broken rules (empty list, if no broken rules)</returns>
        List<BrokenRule> GetBrokenRules(object instance);

        /// <summary>
        /// Checks all business rules for the entity instance and returns a list of broken rules.
        /// (empty list, if no broken rules)
        /// </summary>
        /// <param name="instance">an instance of the entity to check the rules for</param>
        /// <returns>a list of broken rules (empty list, if no broken rules)</returns>
        List<BrokenRule> GetAllBrokenRules(object instance);

        /// <summary>
        /// Gets a list of properties (their names) which validation depends
        /// on the value of the property specified.
        /// </summary>
        /// <param name="propName">a name of the property to get dependent properties for</param>
        /// <returns>a list of properties (their names) which validation depends
        /// on the value of the property specified</returns>
        string[] GetDependentPropertyNames(string propName);

    }
}
