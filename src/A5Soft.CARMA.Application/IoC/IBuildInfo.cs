using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.IoC
{
    /// <summary>
    /// The deployment the app is built as, added to the container by the IoC manager.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IBuildInfo
    {
        /// <summary>Gets the roles of the app build (see <see cref="BuildConfiguration"/>).</summary>
        BuildConfiguration Build { get; }

        /// <summary>Gets whether the app runs the backend in process (a server, or a thick client), so that its use
        /// cases can run locally.</summary>
        bool IncludesBackend { get; }
    }
}
