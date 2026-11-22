using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Domain.UnitTests
{
    /// <summary>
    /// A breach of the framework conventions found by testing (unit test or dynamic plugin tests).
    /// </summary>
    public sealed class ConventionViolation
    {
        private const int MaxListed = 25;


        /// <summary>
        /// Creates a new violation.
        /// </summary>
        /// <param name="subject">the offending type (use case, service, lookup, domain object, query result etc.)</param>
        /// <param name="path">where in its signature or type graph, e.g. <c>parameter 0 (Order).Inner.Deep</c>;
        /// empty for the subject itself</param>
        /// <param name="message">what is wrong</param>
        public ConventionViolation(Type subject, string path, string message)
        {
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            Path = path ?? string.Empty;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }


        /// <summary>Gets the offending type (use case, service, lookup, domain object, query result etc.).</summary>
        public Type Subject { get; }

        /// <summary>Gets where in the subject's signature or type graph; empty for the subject itself.</summary>
        public string Path { get; }

        /// <summary>Gets what is wrong.</summary>
        public string Message { get; }


        /// <inheritdoc />
        public override string ToString()
            => string.IsNullOrEmpty(Path) ? $"{Subject}: {Message}" : $"{Subject}: {Path}: {Message}";


        /// <summary>
        /// Formats a list of violations for an exception message or a log: a heading, then one violation per line,
        /// the first 25 only.
        /// </summary>
        /// <param name="heading">what was checked, e.g. "Plugin X is refused"</param>
        /// <param name="violations">the violations</param>
        public static string Format(string heading, IReadOnlyList<ConventionViolation> violations)
        {
            if (null == violations) throw new ArgumentNullException(nameof(violations));

            var listed = string.Join(Environment.NewLine, violations.Take(MaxListed).Select(v => "  " + v));
            var more = violations.Count > MaxListed
                ? $"{Environment.NewLine}  … and {violations.Count - MaxListed} more"
                : string.Empty;

            return $"{heading}: {violations.Count} convention violation(s):{Environment.NewLine}{listed}{more}";
        }
    }
}
