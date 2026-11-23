using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.IoC
{
    /// <summary>
    /// The build information, added to the container as an instance by the IoC manager.
    /// </summary>
    public sealed class BuildInfo : IBuildInfo
    {
        /// <summary>
        /// Creates new build information.
        /// </summary>
        /// <param name="build">the roles of the app build</param>
        public BuildInfo(BuildConfiguration build) => Build = build;


        /// <inheritdoc />
        public BuildConfiguration Build { get; }

        /// <inheritdoc />
        public bool IncludesBackend => Build.IncludesBackend();
    }
}
