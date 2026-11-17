using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A base class for a domain object that has no identity (a domain singleton or an editable
    /// container, e.g. query criteria or command parameters), with its DTO in the signature.
    /// </summary>
    /// <typeparam name="T">a type of the domain object itself (CRTP)</typeparam>
    /// <typeparam name="TDto">a type of the DTO of the domain object</typeparam>
    /// <remarks>
    /// <para>The compiler enforces the whole contract: the domain object produces its DTO
    /// (<see cref="ToDto"/> is abstract), the DTO rebuilds the domain object
    /// (<see cref="IDomainObjectDto{T}.ToEntity"/> is abstract on the DTO), and the DTO is
    /// paired with this very type (<typeparamref name="TDto"/> is constrained to a DTO of
    /// <typeparamref name="T"/>).</para>
    /// </remarks>
    public abstract class DomainObject<T, TDto> : DomainObject<T>, IDtoSerializable<TDto>
        where T : DomainObject<T, TDto>
        where TDto : class, IDomainObjectDto<T>
    {
        /// <summary>
        /// Creates a new domain object instance.
        /// </summary> 
        /// <param name="context">an app execution context</param>
        protected DomainObject(IApplicationContext context)
            : base(context) { }

        /// <summary>
        /// Creates a domain object instance from its DTO, restoring the state it had when
        /// the DTO was taken.
        /// </summary>
        /// <param name="dto">a DTO to take the state from; the derived class copies the data</param>
        /// <param name="context">an app execution context</param>
        /// <exception cref="ArgumentNullException">the DTO is null</exception>
        /// <remarks>Restores the own state only. Children are restored from their own DTOs,
        /// and the cascading <see cref="ITrackState.IsDirty"/> follows from them.</remarks>
        protected DomainObject(TDto dto, IApplicationContext context)
            : base(context)
        {
            if (null == dto) throw new ArgumentNullException(nameof(dto));

            MarkClean();
        }

        /// <summary>
        /// Creates a copy of the domain object.
        /// </summary>
        /// <param name="source">a domain object to copy</param>
        protected DomainObject(T source) : base(source) { }


        /// <inheritdoc />
        public abstract TDto ToDto();
    }
}
