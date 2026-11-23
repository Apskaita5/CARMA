using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace A5Soft.CARMA.Application.DataPortal.Http
{
    /// <summary>
    /// Encodes the values of the data portal headers to ASCII and back, the same way on the client
    /// and the server. Decoding never throws: a value that cannot be decoded is returned as null, and
    /// the server refuses the call.
    /// </summary>
    public static class DataPortalHeaderValues
    {
        /// <summary>Percent-encodes free text as UTF-8; null for null.</summary>
        /// <param name="value">a text to encode</param>
        public static string EncodeText(string value)
            => null == value ? null : Uri.EscapeDataString(value);

        /// <summary>Decodes percent-encoded text; null if the value is absent or not valid.</summary>
        /// <param name="value">a header value</param>
        public static string DecodeText(string value)
        {
            if (null == value) return null;

            try
            {
                return Uri.UnescapeDataString(value.Trim());
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Encodes a list of names: each percent-encoded (so a comma inside a generic type name is
        /// escaped), joined by commas.
        /// </summary>
        /// <param name="values">the names to encode</param>
        public static string EncodeList(IEnumerable<string> values)
            => string.Join(",", (values ?? Enumerable.Empty<string>()).Select(EncodeText));

        /// <summary>Decodes a list of names; an empty array for an absent or empty value.</summary>
        /// <param name="value">a header value</param>
        public static string[] DecodeList(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return Array.Empty<string>();

            return value.Split(',').Select(DecodeText).ToArray();
        }

        /// <summary>Formats a non-negative number.</summary>
        /// <param name="value">a number to format</param>
        public static string EncodeNumber(long value) => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>Parses a non-negative number; null if the value is absent or not valid.</summary>
        /// <param name="value">a header value</param>
        public static long? DecodeNumber(string value)
            => long.TryParse(value?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var result)
                ? result
                : (long?)null;
    }
}
