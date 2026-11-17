using System;
using System.Collections.Generic;
using System.Text;

namespace A5Soft.CARMA.Domain.Rules
{
    /// <summary>
    /// Extension methods for common validation tasks.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Validates a POCO value. Returns an empty collection if the value to validate is
        /// null or not a class or an interface.
        /// </summary>
        /// <typeparam name="T">a type of the value to validate</typeparam>
        /// <param name="validationEngine"></param>
        /// <param name="valueToValidate">value to validate</param>
        public static BrokenRulesCollection ValidatePoco<T>(this IValidationEngineProvider validationEngine,
            T valueToValidate)
        {
            if (null == validationEngine) throw new ArgumentNullException(nameof(validationEngine));

            if (null == valueToValidate) return new BrokenRulesCollection();

            var pocoType = typeof(T);
            if (!pocoType.IsInterface && !pocoType.IsClass) return new BrokenRulesCollection();
            if (pocoType == typeof(string)) return new BrokenRulesCollection();

            var result = validationEngine.GetValidationEngine(pocoType)
                .GetAllBrokenRules(valueToValidate);

            return new BrokenRulesCollection(result);
        }
    }
}
