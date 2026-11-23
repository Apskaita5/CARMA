using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.IoC
{
    /// <summary>
    /// Helpers for <see cref="BuildConfiguration"/>.
    /// </summary>
    public static class BuildConfigurationExtensions
    {
        /// <summary>
        /// Gets whether an implementation meant for the roles given is used by an app build: they share a role.
        /// </summary>
        /// <param name="implementation">the roles the implementation is meant for</param>
        /// <param name="build">the roles of the app build</param>
        public static bool Serves(this BuildConfiguration implementation, BuildConfiguration build)
            => (implementation & build) != 0;

        /// <summary>
        /// Gets whether a value is one of the four app builds: <see cref="BuildConfiguration.Server"/>,
        /// <see cref="BuildConfiguration.Client"/>, Client | Server (an ASP.NET server with a web UI) or
        /// Client | Local (a thick client).
        /// </summary>
        public static bool IsAppBuild(this BuildConfiguration build)
            => build == BuildConfiguration.Server
               || build == BuildConfiguration.Client
               || build == (BuildConfiguration.Client | BuildConfiguration.Server)
               || build == (BuildConfiguration.Client | BuildConfiguration.Local);

        /// <summary>
        /// Gets whether an app build runs the backend in process (a server, or a thick client), so that its use
        /// cases can run locally.
        /// </summary>
        public static bool IncludesBackend(this BuildConfiguration build)
            => BuildConfiguration.Backend.Serves(build);
    }
}
