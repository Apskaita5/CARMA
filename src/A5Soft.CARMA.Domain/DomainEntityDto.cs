using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A default implementation of <see cref="IDomainEntityDto{T}"/>: a base class for the DTO
    /// of a domain entity. Carries the identity and the state flags.
    /// </summary>
    /// <typeparam name="T">a type of the domain entity the DTO carries data for</typeparam>
    /// <remarks>
    /// <para><see cref="DomainEntityIdentity{T}"/> is serializable, so it is carried as it is.</para>
    /// </remarks>
    [Serializable]
    public abstract class DomainEntityDto<T> : DomainObjectDto<T>, IDomainEntityDto<T>
        where T : class
    {
        /// <summary>
        /// Creates a new empty DTO instance (for deserializers and repositories).
        /// </summary>
        protected DomainEntityDto() { }

        /// <summary>
        /// Creates a new DTO instance with the identity and the state of the domain entity given.
        /// </summary>
        /// <param name="entity">a domain entity to take the identity and the state from</param>
        /// <exception cref="ArgumentNullException">the domain entity is null</exception>
        protected DomainEntityDto(T entity) : base(entity)
        {
            if (entity is IDomainEntity<T> domainEntity)
            {
                Id = domainEntity.Id;
                IsNew = domainEntity.IsNew;
            }
        }


        /// <inheritdoc />
        public DomainEntityIdentity<T> Id { get; set; }

        /// <inheritdoc />
        public bool IsNew { get; set; }
    }
}
