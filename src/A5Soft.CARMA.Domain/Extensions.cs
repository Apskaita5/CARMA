using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Extensions for common application functionality.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns a string split by camel case convention, e.g. TotalAmount -> Total Amount
        /// </summary>
        /// <param name="input">string to split</param>
        /// <returns></returns>
        internal static string SplitCamelCase(this string input)
        {
            if (input.IsNullOrWhiteSpace()) return string.Empty;
            var result = Regex.Replace(Regex.Replace(input, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), 
                @"(\p{Ll})(\P{Ll})", "$1 $2");
            if (result.Length > 1) result = result.First().ToString().ToUpper() + result.Substring(1);
            else result = result.ToUpper();
            return result;
        }

        /// <summary>
        /// Returns a value indicating that the object (value) is null. Required due to potential operator overloadings
        /// that cause unpredictable behaviour of standard null == value test.
        /// </summary>
        /// <typeparam name="T">a type of the object to test</typeparam>
        /// <param name="value">an object to test against null</param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static bool IsNull<T>(this T value) where T : class
        {
            return ReferenceEquals(value, null) || DBNull.Value == value;
        }

        /// <summary>
        /// Returns a value indicating whether the reference is to the same domain entity.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="referenceToCompare"></param>
        /// <returns>a value indicating whether the reference is to the same domain entity</returns>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static bool ReferenceEqualsTo(this IDomainEntityReference reference, 
            IDomainEntityReference referenceToCompare)
        {
            if (ReferenceEquals(reference, null) && object.ReferenceEquals(referenceToCompare, null))
                return true;
            if (ReferenceEquals(reference, null) || object.ReferenceEquals(referenceToCompare, null))
                return false;

            return (referenceToCompare.DomainEntityType == reference.DomainEntityType
                && referenceToCompare.DomainEntityKey.Equals(reference.DomainEntityKey, 
                    StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the identity specified is null.
        /// </summary>
        /// <typeparam name="T">a type of entity that is expected to be referenced</typeparam>
        /// <param name="id">an identity to check</param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static void EnsureValidIdentityFor<T>(this DomainEntityIdentity<T> id)
        {
            if (null == id) throw new ArgumentException(
                $"The identity does not reference any existing domain entity.", nameof(id));
        }

        /// <summary>
        /// Returns true if the string value is null or empty or consists from whitespaces only.
        /// </summary>
        /// <param name="value">a string value to evaluate</param> 
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return (null == value || string.IsNullOrEmpty(value.Trim()));
        }

        /// <summary>
        /// Gets a dictionary value by key or null, if no such key in the dictionary.
        /// </summary>
        /// <typeparam name="T">type of dictionary values</typeparam>
        /// <param name="dictionary">dictionary to search</param>
        /// <param name="key">key to find</param>
        /// <returns>a dictionary value by key or null, if no such key in the dictionary</returns> 
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static T ValueOrDefault<T>(this IDictionary<string, T> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out var result)) return result;
            return default;
        }

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

        /// <summary>
        /// Gets a value indicating whether <paramref name="type"/> value can be assigned to
        /// <paramref name="destinationType"/> variable including implicit conversions.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="destinationType">type to check assignability to</param>
        /// <returns>a value indicating whether <paramref name="type"/> value can be assigned to
        /// <paramref name="destinationType"/> variable including implicit conversions</returns>
        /// <remarks><see cref="Type.IsAssignableFrom"/> ignores implicit conversions</remarks>
        public static bool IsConvertableTo(this Type type, Type destinationType)
        {
            if (null == type) throw new ArgumentNullException(nameof(type));
            if (null == destinationType) throw new ArgumentNullException(nameof(destinationType));

            if (type == destinationType || destinationType.IsAssignableFrom(type))
                return true;

            return (from method in type.GetMethods(BindingFlags.Static |
                                                   BindingFlags.Public |
                                                   BindingFlags.FlattenHierarchy)
                    where method.Name == "op_Implicit" &&
                          method.ReturnType == destinationType
                    select method
                    ).Count() > 0;
        }
    }
}
