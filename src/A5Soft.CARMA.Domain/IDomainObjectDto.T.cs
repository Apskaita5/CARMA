using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A contract of the DTO of a domain object of type <typeparamref name="T"/>: a domain
    /// object that has no identity (a domain singleton or an editable container, e.g. query
    /// criteria or command parameters).
    /// </summary>
    /// <typeparam name="T">a type of the domain object the DTO carries data for</typeparam>
    /// <remarks>
    /// <para>The type parameter is covariant, so that code that only rebuilds domain objects
    /// can treat DTOs of related types alike.</para>
    /// <para>The type-erased <see cref="IDomainObjectDto.ToEntity"/> is implemented by this
    /// interface itself (a default interface method, C# 8 / .NET Standard 2.1), so a DTO only
    /// ever implements the strongly typed method.</para>
    /// </remarks>
    public interface IDomainObjectDto<out T> : IDomainObjectDto
        where T : class
    {
        /// <summary>
        /// Creates a domain object instance from the data of this DTO.
        /// </summary>
        /// <param name="validationEngineProvider">a validation engine provider for the new
        /// instance; the local one, so that a domain object never carries the rules of the
        /// side it came from</param>
        /// <returns>a new domain object instance with the data and the state of this DTO</returns>
        /// <remarks>Implement as a single line that calls the domain object constructor taking
        /// the DTO: <c>=&gt; new Invoice(this, validationEngineProvider);</c>. Together with
        /// <see cref="IDtoSerializable{TDto}.ToDto"/> on the domain object, this makes both
        /// directions of the mapping a compile time requirement.</remarks>
        new T ToEntity(IValidationEngineProvider validationEngineProvider);

        /// <inheritdoc cref="IDomainObjectDto.ToEntity"/>
        object IDomainObjectDto.ToEntity(IValidationEngineProvider validationEngineProvider)
            => ToEntity(validationEngineProvider);
    }
}
