using System.Linq;
using System.Text.RegularExpressions;

namespace A5Soft.CARMA.Domain.Metadata.DataAnnotations
{
    /// <summary>
    /// Extension methods for common data annotation tasks.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns a string split by camel case convention, e.g. TotalAmount -> Total Amount
        /// </summary>
        /// <param name="input">string to split</param>
        /// <returns>a string split by camel case convention, e.g. TotalAmount -> Total Amount</returns>
        internal static string SplitCamelCase(this string input)
        {
            if (input.IsNullOrWhiteSpace()) return string.Empty;
            var result = Regex.Replace(Regex.Replace(input, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"),
                @"(\p{Ll})(\P{Ll})", "$1 $2");
            if (result.Length > 1) result = result.First().ToString().ToUpper() + result.Substring(1);
            else result = result.ToUpper();
            return result;
        }
    }
}
