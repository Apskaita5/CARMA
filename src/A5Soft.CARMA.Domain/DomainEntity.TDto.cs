using System;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A base class for a domain entity, with its DTO in the signature.
    /// </summary>
    /// <typeparam name="T">a type of the domain entity itself</typeparam>
    /// <typeparam name="TDto">a type of the DTO of the domain entity</typeparam>
    /// <remarks>
    /// <para>The compiler enforces the whole contract: the domain entity produces its DTO
    /// (<see cref="ToDto"/> is abstract), the DTO rebuilds the domain entity
    /// (<see cref="IDomainObjectDto{T}.ToEntity"/> is abstract on the DTO), and the DTO is
    /// paired with this very type (<typeparamref name="TDto"/> is constrained to a DTO of
    /// <typeparamref name="T"/>).</para>
    /// <para>Declare the DTO as a nested class: <c>public class Invoice :
    /// DomainEntity&lt;Invoice, Invoice.Dto&gt;</c>.</para>
    /// <para>The constructor taking the DTO replaces the one taking an identity.</para>
    /// </remarks>
    public abstract class DomainEntity<T, TDto> : DomainEntity<T>, IDtoSerializable<TDto>
        where T : DomainEntity<T, TDto>
        where TDto : class, IDomainEntityDto<T>
    {
        /// <summary>
        /// Creates a new domain entity instance.
        /// </summary>
        /// <param name="context">an app execution context</param>
        protected DomainEntity(IDomainContext context)
            : base(context) { }

        /// <summary>
        /// Creates a domain entity instance from its DTO, restoring the identity and the state
        /// it had when the DTO was taken.
        /// </summary>
        /// <param name="dto">a DTO to take the identity and the state from</param>
        /// <param name="context">an app execution context</param>
        /// <exception cref="ArgumentNullException">the DTO is null</exception>
        /// <remarks>
        /// <para>A DTO without an identity describes a new domain entity: it is restored as
        /// a new instance, which is always dirty, whatever the DTO flag says.</para>
        /// <para>A DTO with an identity describes a persisted domain entity: it is restored as
        /// clean.</para>
        /// <para>Restores the own state only. Children are restored from their own DTOs,
        /// and the cascading <see cref="ITrackState.IsDirty"/> follows from them.</para>
        /// </remarks>
        protected DomainEntity(TDto dto, IDomainContext context)
            : base(context)
        {
            if (null == dto) throw new ArgumentNullException(nameof(dto));

            // the identity alone decides IsNew; IsNew of the DTO is never trusted
            if (null == dto.Id) return;

            MarkOld(dto.Id);
            MarkClean();
        }

        /// <summary>
        /// Creates a copy of the domain entity.
        /// </summary>
        /// <param name="source">a domain entity to copy</param>
        protected DomainEntity(T source) : base(source) { }


        /// <inheritdoc />
        public abstract TDto ToDto();
    }
}
