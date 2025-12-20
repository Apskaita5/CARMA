using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Interface for validation rule provider for use in dependency injection.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IRuleProvider
    {
        /// <summary>
        /// Gets a configured rule implementation for the specified interface type.
        /// </summary>
        /// <param name="ruleInterfaceType">a type of the rule interface</param>
        /// <returns>a configured rule implementation for the specified interface type</returns>
        IValidationRule ResolveRule(Type ruleInterfaceType);
    }
}
