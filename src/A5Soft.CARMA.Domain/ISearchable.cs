namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Interface to denote a class (lookup, query result, etc.) as searchable.
    /// </summary>
    public interface ISearchable
    {
        /// <summary>
        /// Gets a value indicating whether the item matches the search string.
        /// </summary>
        /// <param name="searchString">a search string to compare to</param>
        /// <returns>a value indicating whether the item matches the search string</returns>
        bool Match(string searchString);
    }
}
