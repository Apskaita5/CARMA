using System;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A default implementation of <see cref="IDomainObjectDto{T}"/>: a base class for the DTO
    /// of a domain object that has no identity (a domain singleton or an editable container,
    /// e.g. query criteria or command parameters).
    /// </summary>
    /// <typeparam name="T">a type of the domain object the DTO carries data for</typeparam>
    [Serializable]
    public abstract class DomainObjectDto<T> : IDomainObjectDto<T>
        where T : class
    {
        /// <summary>
        /// Creates a new empty DTO instance (for deserializers and repositories).
        /// </summary>
        /// <remarks>The state flag stays false, so a domain object built from a DTO that
        /// a repository has filled is clean.</remarks>
        protected DomainObjectDto() { }

        /// <summary>
        /// Creates a new DTO instance with the state of the domain object given.
        /// </summary>
        /// <param name="domainObject">a domain object to take the state from; a domain object
        /// that does not track state is described as unchanged</param>
        /// <exception cref="ArgumentNullException">the domain object is null</exception>
        protected DomainObjectDto(T domainObject)
        {
            if (null == domainObject) throw new ArgumentNullException(nameof(domainObject));

            // own state only: cascading is recomputed by the domain object from its children
            if (domainObject is ITrackState state) IsDirty = state.IsSelfDirty;
        }


        /// <inheritdoc />
        public bool IsDirty { get; set; }


        /// <inheritdoc />
        public abstract T ToEntity(IValidationEngineProvider validationEngineProvider);
    }
}
