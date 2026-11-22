using System;
using System.Collections.Generic;
using System.Globalization;

namespace A5Soft.CARMA.Application.DataPortal.Internal
{
    /// <summary>
    /// Collects the problems of a configuration section, so that all of them are reported at once.
    /// </summary>
    internal sealed class OptionsValidation
    {
        public const long KB = 1024;
        public const long MB = 1024 * KB;
        public const long GB = 1024 * MB;

        private readonly string _section;
        private readonly List<string> _problems = new List<string>();


        public OptionsValidation(string section) => _section = section;


        /// <summary>Checks that a value lies within the range given.</summary>
        public void Range(long value, long min, long max, string name)
        {
            if (value < min || value > max)
                _problems.Add($"{name} must be between {Format(min)} and {Format(max)}, it is {Format(value)}");
        }

        /// <summary>Records a problem when the condition does not hold.</summary>
        public void Require(bool condition, string problem)
        {
            if (!condition) _problems.Add(problem);
        }

        /// <summary>Throws if any problem has been recorded.</summary>
        /// <exception cref="InvalidOperationException">the message lists every problem</exception>
        public void ThrowIfInvalid()
        {
            if (_problems.Count > 0) throw new InvalidOperationException(
                $"The {_section} configuration is not valid: {string.Join("; ", _problems)}.");
        }


        /// <summary>Formats a number, with its size in KB, MB or GB when it is a whole multiple.</summary>
        public static string Format(long value)
        {
            var text = value.ToString(CultureInfo.InvariantCulture);

            if (value >= GB && value % GB == 0) return $"{text} ({value / GB} GB)";
            if (value >= MB && value % MB == 0) return $"{text} ({value / MB} MB)";
            if (value >= KB && value % KB == 0) return $"{text} ({value / KB} KB)";

            return text;
        }
    }
}
