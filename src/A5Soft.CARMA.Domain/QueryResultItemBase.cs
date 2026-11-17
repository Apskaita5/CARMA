using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Base for query results that are lists of items, e.g. a report with multiple rows.
    /// Setters must be protected so the ORM can populate them by inheritance and nothing else can.
    /// </summary>
    /// <remarks>Query results cross the repository boundary, which may be on another server,
    /// so every concrete result needs the private all-properties serialization constructor
    /// (see <see cref="SerializationConstructorAttribute"/>).</remarks>
    [Serializable]
    public abstract class QueryResultItemBase : QueryResultBase
    {
        /// <summary>Client side search support.</summary>
        public abstract bool Match(string searchString);


        /// <summary>
        /// Gets a value indicating whether the search string is contained in any of the values given.
        /// </summary>
        /// <param name="searchString">a string to search for</param>
        /// <param name="valuesToSearch">values to search in</param>
        /// <returns>true if the search string is found in any of the values given</returns>
        /// <remarks>Commonly used by <see cref="Match(string)"/> implementations.
        /// <paramref name="valuesToSearch"/> are basicaly string props used in search.
        /// Comparison is culture aware and case insensitive, which is what a user typing into a filter box
        /// expects.</remarks>
        protected static bool IsFoundInAny(string searchString, params string[] valuesToSearch)
        {
            if (searchString.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(searchString));

            if (null == valuesToSearch || valuesToSearch.Length < 1) return false;

            foreach (var valueToSearch in valuesToSearch)
            {
                if (!valueToSearch.IsNullOrWhiteSpace() && valueToSearch.IndexOf(
                    searchString, StringComparison.CurrentCultureIgnoreCase) > -1) return true;
            }

            return false;
        }
    }
}
