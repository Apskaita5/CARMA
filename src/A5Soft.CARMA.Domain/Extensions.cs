using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace A5Soft.CARMA.Domain
{
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
        /// Gets a SHA256 hash value for a string.
        /// </summary>
        /// <param name="rawData">a string to compute SHA256 hash for</param>
        public static string ComputeSha256Hash(this string rawData)
        {
            if (rawData.IsNullOrWhiteSpace()) return string.Empty;
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


        public static bool IsSameIdentityAs(this IDomainEntityIdentity firstIdentity, IDomainEntityIdentity identity)
        {
            if (firstIdentity.IsNull()) throw new ArgumentNullException(nameof(firstIdentity));
            if (identity.IsNull()) throw new ArgumentNullException(nameof(identity));

            if (firstIdentity.DomainEntityType != identity.DomainEntityType) return false;

            if (firstIdentity.IsNew != identity.IsNew) return false;

            if (firstIdentity.IsNew) return ReferenceEquals(firstIdentity, identity);

            return firstIdentity.CompareTo(identity) == 0;
        }

        public static bool IsValidKey(this string value)
        {
            return !value.IsNullOrWhiteSpace();
        }

        public static bool IsValidKey(this int value)
        {
            return value > 0;
        }

        public static bool IsValidKey(this int? value)
        {
            return value.HasValue && value.Value > 0;
        }

        public static bool IsValidKey(this long value)
        {
            return value > 0;
        }

        public static bool IsValidKey(this long? value)
        {
            return value.HasValue && value.Value > 0;
        }

        public static bool IsValidKey(this Guid value)
        {
            return value != Guid.Empty;
        }

        public static bool IsValidKey(this Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty;
        }

    }
}
