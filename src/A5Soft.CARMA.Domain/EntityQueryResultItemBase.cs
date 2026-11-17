using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Base for query results that are lists of items referencing particular domain entities, e.g. a list of invoices.
    /// Setters must be protected so the ORM can populate them by inheritance and nothing else can.
    /// </summary>
    /// <remarks>Query results cross the repository boundary, which may be on another server,
    /// so every concrete result needs the private all-properties serialization constructor
    /// (see <see cref="SerializationConstructorAttribute"/>).</remarks>
    [Serializable]
    public abstract class EntityQueryResultItemBase<T> : QueryResultItemBase
        where T : class
    {
        /// <summary>For the ORM at DAL level.</summary>
        protected EntityQueryResultItemBase() { }

        /// <summary>For serialization.</summary>
        protected EntityQueryResultItemBase(DomainEntityIdentity<T> id)
        {
            Id = id;
        }


        /// <summary>
        /// Identity of the entity behind the query result.
        /// Not displayed - it exists to run commands against the underlying entity,
        /// so it carries no Display attribute.
        /// </summary>
        public DomainEntityIdentity<T> Id { get; protected set; }
    }
}
