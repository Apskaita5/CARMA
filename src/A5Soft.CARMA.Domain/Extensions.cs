using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Extensions for common application functionality.
    /// </summary>
    public static class Extensions
    {
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
        /// Traverses the exception hierarchy and returns the first exception of type
        /// <see cref="BusinessException"/>. If no such exception, returns null;
        /// </summary>
        /// <param name="ex"></param> 
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static BusinessException ToBusinessException(this Exception ex)
        {
            if (null == ex) return null;

            if (ex is BusinessException bex) return bex;

            if (ex is AggregateException aex)
            {
                foreach (var e in aex.InnerExceptions)
                {
                    var result = e.ToBusinessException();
                    if (null != result) return result;
                }
            }
            else if (null != ex.InnerException)
            {
                return ex.InnerException.ToBusinessException();
            }

            return null;
        }

        /// <summary>
        /// Gets an extensive description of an exception.
        /// </summary>
        /// <param name="ex">an exception to get a description for</param>
        /// <returns>an extensive description of an exception</returns>
        [DebuggerHidden]
        [DebuggerStepThrough]
        internal static string GetFullDescription(this Exception ex)
        {
            if (null == ex) return string.Empty;

            var builder = new StringBuilder();
            ex.AddInstanceDescription(builder);
            if (null != ex.InnerException) ex.InnerException.AddFullDescription(builder);

            return builder.ToString();
        }

        private static void AddFullDescription(this Exception ex, StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine("------Internal Exception------");
            builder.AppendLine();
            ex.AddInstanceDescription(builder);
            if (null != ex.InnerException) ex.InnerException.AddFullDescription(builder);
        }

        private static void AddInstanceDescription(this Exception ex, StringBuilder builder)
        {
            builder.AppendLine(ex.Message);
            builder.AppendLine($"Exception type: {ex.GetType().Name}");
            builder.AppendLine($"Target site: {ex.TargetSite?.Name}");
            builder.AppendLine("Stack trace:");
            builder.AppendLine(ex.StackTrace);
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
    }
}
