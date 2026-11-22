namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A type-erased contract of a domain object DTO, for code that handles DTOs without
    /// knowing the domain object type (the data portal first of all).
    /// </summary>
    /// <remarks>A DTO is the only form in which a domain object crosses a serialization boundary
    /// or travels to and from a repository. It is a plain data container: every member that
    /// a serializer or a repository writes has a public setter on the concrete class.
    /// The interfaces expose only what the framework reads.</remarks>
    public interface IDomainObjectDto
    {
        /// <summary>
        /// Gets a value indicating whether the data of the domain object itself has changed.
        /// </summary>
        /// <remarks>
        /// <para>Taken from <see cref="ITrackState.IsSelfDirty"/>, never from the cascading
        /// <see cref="ITrackState.IsDirty"/>. Cascading is a concern of the domain object, which
        /// recomputes it from its children; every child DTO carries its own flag, so a DTO
        /// only ever describes itself.</para>
        /// <para>For a repository this is the per row answer to "does this row need an update"</para>
        /// <para>A DTO filled by a repository leaves the flag false: the domain object built
        /// from it is clean.</para>
        /// </remarks>
        bool IsDirty { get; }

        /// <summary>
        /// Creates a domain object instance from the data of this DTO.
        /// </summary>
        /// <param name="context">the application context of the side that builds the instance,
        /// so that a domain object never carries the rules, metadata or binding mode of the side
        /// it came from</param>
        /// <returns>a new domain object instance with the data and the state of this DTO</returns>
        object ToEntity(IApplicationContext context);
    }
}
