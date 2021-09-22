using System;
using System.Collections.Generic;
using System.Linq;
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
        public static bool IsNull<T>(this T value) where T : class
        {
            return ReferenceEquals(value, null) || DBNull.Value == value;
        }

        /// <summary>
        /// Returns a value indicating whether the identity is either null or a new identity
        /// (see <see cref="IDomainEntityIdentity.IsNew"/>).
        /// </summary>
        /// <param name="identity">the identity to check</param>
        /// <returns>a value indicating whether the identity is either null or a new identity</returns>
        public static bool IsNullOrNew(this IDomainEntityIdentity identity)
        {
            return ReferenceEquals(identity, null) || identity.IsNew;
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the identity specified is either null 
        /// or does not reference an existing entity of type T (see <see cref="IDomainEntityIdentity.IsNew"/>
        /// and <see cref="IDomainEntityIdentity.DomainEntityType"/>).
        /// </summary>
        /// <typeparam name="T">a type of entity that is expected to be referenced</typeparam>
        /// <param name="id">an identity to check</param>
        public static void EnsureValidIdentityFor<T>(this IDomainEntityIdentity id)
        {
            if (id.IsNullOrNew()) throw new ArgumentException(
                $"The identity does not reference any existing domain entity.", nameof(id));
            if (null == id.DomainEntityType) throw new InvalidOperationException(
                $"{nameof(IDomainEntityIdentity.DomainEntityType)} property cannot be null.");
            if (!id.DomainEntityType.IsAssignableFrom(typeof(T))) throw new ArgumentException(
                $"Required identity for {typeof(T).FullName} while received {id.DomainEntityType.FullName}.",
                nameof(id));
        }

        /// <summary>
        /// Returns true if the string value is null or empty or consists from whitespaces only.
        /// </summary>
        /// <param name="value">a string value to evaluate</param>
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
        public static T ValueOrDefault<T>(this IDictionary<string, T> dictionary, string key)
        {
            if (dictionary.ContainsKey(key)) return dictionary[key];
            return default;
        }

        /// <summary>
        /// Gets a value indicating whether the identity references the same entity.
        /// </summary>
        /// <param name="firstIdentity"></param>
        /// <param name="identity">the identity to check</param>
        public static bool IsSameIdentityAs(this IDomainEntityIdentity firstIdentity, IDomainEntityIdentity identity)
        {
            if (null == firstIdentity && null == identity) return true;
            if (null == firstIdentity || null == identity) return false;

            if (firstIdentity.DomainEntityType != identity.DomainEntityType) return false;

            if (firstIdentity.IsNew != identity.IsNew) return false;

            if (firstIdentity.IsNew) return true;

            return firstIdentity.CompareTo(identity) == 0;
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this string value)
        {
            return !value.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this int value)
        {
            return value > 0;
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this int? value)
        {
            return value.HasValue && value.Value > 0;
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this long value)
        {
            return value > 0;
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this long? value)
        {
            return value.HasValue && value.Value > 0;
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this Guid value)
        {
            return value != Guid.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether the value is a (potentially) valid primary key.
        /// </summary>
        /// <param name="value"></param>
        public static bool IsValidKey(this Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty;
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
            if (pocoType == typeof(string) || typeof(ILookup).IsAssignableFrom(pocoType)) 
                return new BrokenRulesCollection();

            var result = validationEngine.GetValidationEngine(pocoType)
                .GetAllBrokenRules(valueToValidate);

            return new BrokenRulesCollection(result);
        }

    }
}
