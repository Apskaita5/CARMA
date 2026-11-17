namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Implemented by a domain object that crosses a serialization boundary as its DTO.
    /// </summary>
    /// <typeparam name="TDto">a type of the DTO that carries the domain object data</typeparam>
    /// <remarks>The type parameter is covariant, so the data portal (or any other type-erased code)
    /// can use any domain object as an <c>IDtoSerializable&lt;IDomainObjectDto&gt;</c> without
    /// reflection.</remarks>
    public interface IDtoSerializable<out TDto>
        where TDto : class, IDomainObjectDto
    {
        /// <summary>
        /// Gets a new DTO that contains the current data and state of the domain object.
        /// </summary>
        /// <returns>a new DTO instance; never the same instance twice, so that a caller can
        /// change it freely</returns>
        /// <remarks>Implement as a single line that calls the DTO constructor taking the domain
        /// object: <c>=&gt; new Dto(this);</c>. Child domain objects are converted by their own
        /// <see cref="ToDto"/>, each carrying its own state.</remarks>
        TDto ToDto();
    }
}
