using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Base for query results. Setters must be protected so the ORM can populate
    /// them by inheritance and nothing else can.
    /// </summary>
    /// <remarks>Query results cross the repository boundary, which may be on another server,
    /// so every concrete result needs the private all-properties serialization constructor
    /// (see <see cref="SerializationConstructorAttribute"/>).</remarks>
    [Serializable]
    public abstract class QueryResultBase
    {
        /// <summary>For the ORM at DAL level.</summary>
        protected QueryResultBase() { }


        /// <summary>
        /// Recalculates any derived columns. Virtual and declared here so a caller holding only
        /// the base - a repository looping over rows it just materialized - can invoke it without
        /// knowing the concrete result type. Rows with no derived columns simply do nothing.
        /// </summary>
        public virtual void CalculateSubtotals() { }
    }
}
