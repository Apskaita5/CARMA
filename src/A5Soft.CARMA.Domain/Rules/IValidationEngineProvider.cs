using System;

namespace A5Soft.CARMA.Domain.Rules
{
    /// <summary>
    /// Common interface for validation providers (services) for use in dependency injection.
    /// </summary>
    public interface IValidationEngineProvider
    {

        /// <summary>
        /// Gets a validation engine for the entity type specified.
        /// </summary>
        /// <param name="entityType">a type of the entity to get a validation engine for</param>
        /// <returns>validation engine for the entity type specified</returns>
        IValidationEngine GetValidationEngine(Type entityType);

        /// <summary>
        /// Gets a validation engine for the entity type specified.
        /// </summary>
        /// <typeparam name="T">a type of the entity to get a validation engine for</typeparam>
        /// <returns>validation engine for the entity type specified</returns>
        IValidationEngine GetValidationEngine<T>() where T : class;

    }
}
