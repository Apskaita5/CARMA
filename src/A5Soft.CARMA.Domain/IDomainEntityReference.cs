using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Allows to compare references to domain entities. 
    /// </summary>
    public interface IDomainEntityReference
    {
        /// <summary>
        /// A type of the domain entity referenced.
        /// </summary>
        Type DomainEntityType { get; }

        /// <summary>
        /// A (primary) key of the domain entity referenced.
        /// </summary>
        string DomainEntityKey { get; }
    }
}
