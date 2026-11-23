using A5Soft.CARMA.Domain.Files;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace A5Soft.CARMA.Application.DataPortal.Protocol
{
    /// <summary>
    /// Extension method for sanitizing string values.
    /// </summary>
    public static class Sanitizer
    {
        private const int MaxFileNameBytes = 255;

        // Invalid on Windows. ':' also blocks drive-relative paths ("C:x") and NTFS streams ("a.txt:s").
        private static readonly char[] InvalidFileNameChars = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

        private static readonly HashSet<string> ReservedWindowsNames =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };


        /// <summary>
        /// A file name from the client is untrusted: only its last segment is kept, without control or
        /// invisible formatting characters, with characters that are invalid or have path meaning on
        /// Windows replaced, and limited to 255 UTF-8 bytes, so it can never name a path on any OS.
        /// It is still not safe to concatenate into a header; use RFC 5987 encoding for that.
        /// The use case still decides where (and whether) to store the file.
        /// </summary>
        public static string SanitizeFileName(string fileName, string defaultFileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return defaultFileName;

            var name = fileName.Substring(Math.Max(fileName.LastIndexOf('/'), fileName.LastIndexOf('\\')) + 1);

            var result = new StringBuilder(name.Length);
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];

                if (char.IsSurrogate(c))
                {
                    // Keep well-formed pairs only; a lone surrogate is invalid UTF-16.
                    if (char.IsHighSurrogate(c) && i + 1 < name.Length && char.IsLowSurrogate(name[i + 1]))
                    {
                        result.Append(c).Append(name[i + 1]);
                        i++;
                    }
                    continue;
                }

                if (IsInvisibleOrControl(c)) continue;

                result.Append(Array.IndexOf(InvalidFileNameChars, c) >= 0 ? '_' : c);
            }

            // Windows silently drops trailing dots and spaces, so names that differ only there would collide.
            name = result.ToString().Trim().TrimEnd('.', ' ');

            name = KeepTailWithinUtf8Bytes(name, MaxFileNameBytes).TrimStart();
            if (IsReservedWindowsName(name)) name = "_" + KeepTailWithinUtf8Bytes(name, MaxFileNameBytes - 1);

            // "." and ".." are already empty here (trailing dots were trimmed).
            return name.Length < 1 ? defaultFileName : name;
        }

        /// <summary>
        /// A media type from the client is untrusted: only a plain <c>type/subtype</c> of RFC 7230 token
        /// characters is kept, lower-cased and without parameters, so it is safe to place in a header.
        /// It says nothing about whether the content is safe to serve inline.
        /// </summary>
        public static string SanitizeContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType)) return FileContentType.Binary;

            var semicolon = contentType.IndexOf(';');
            var mediaType = (semicolon < 0 ? contentType : contentType.Substring(0, semicolon)).Trim();
            if (mediaType.Length > 200) return FileContentType.Binary;

            var slash = mediaType.IndexOf('/');
            if (slash < 1 || slash == mediaType.Length - 1) return FileContentType.Binary;

            for (var i = 0; i < mediaType.Length; i++)
            {
                // '/' is not a token character, so a second slash is rejected here too.
                if (i != slash && !IsTokenChar(mediaType[i])) return FileContentType.Binary;
            }

            return mediaType.ToLowerInvariant();
        }


        private static bool IsTokenChar(char c) =>
            (c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') ||
            (c >= '0' && c <= '9') ||
            "!#$%&'*+-.^_`|~".IndexOf(c) >= 0;

        private static bool IsInvisibleOrControl(char c)
        {
            switch (char.GetUnicodeCategory(c))
            {
                case UnicodeCategory.Control:            // C0, DEL, C1
                case UnicodeCategory.Format:             // RLO (U+202E), zero-width chars, BOM, soft hyphen
                case UnicodeCategory.LineSeparator:      // U+2028
                case UnicodeCategory.ParagraphSeparator: // U+2029
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Device names are reserved with any extension: "nul.txt" and "CON .tar.gz" included.</summary>
        private static bool IsReservedWindowsName(string name)
        {
            var dot = name.IndexOf('.');
            var stem = (dot < 0 ? name : name.Substring(0, dot)).TrimEnd();
            return ReservedWindowsNames.Contains(stem);
        }

        /// <summary>
        /// Keeps the end of the value (so the extension survives) within a UTF-8 byte budget, never
        /// splitting a surrogate pair. Assumes the value contains no lone surrogates.
        /// </summary>
        private static string KeepTailWithinUtf8Bytes(string value, int maxBytes)
        {
            if (Encoding.UTF8.GetByteCount(value) <= maxBytes) return value;

            var bytes = 0;
            var start = value.Length;
            while (start > 0)
            {
                var c = value[start - 1];
                var isPair = char.IsLowSurrogate(c) && start > 1 && char.IsHighSurrogate(value[start - 2]);
                var charBytes = isPair ? 4 : c < 0x80 ? 1 : c < 0x800 ? 2 : 3;

                if (bytes + charBytes > maxBytes) break;

                bytes += charBytes;
                start -= isPair ? 2 : 1;
            }

            return value.Substring(start);
        }
    }
}
