using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Designates the default implementation of a service or use case interface, to be added to the IoC container
    /// by the startup scan, in the app builds it is meant for.
    /// </summary>
    /// <remarks>
    /// <para><b>Choosing the build configuration.</b> An implementation is registered in an app build if they share
    /// a role (<see cref="BuildConfiguration"/>). An app is built as one of four deployments: a server
    /// (<see cref="BuildConfiguration.Server"/>), an ASP.NET server that also serves a web UI
    /// (<see cref="BuildConfiguration.Client"/> | <see cref="BuildConfiguration.Server"/>), a thin client
    /// (<see cref="BuildConfiguration.Client"/>), or a thick client that can also work locally
    /// (<see cref="BuildConfiguration.Client"/> | <see cref="BuildConfiguration.Local"/>). Ask where the
    /// implementation is needed, and pick the narrowest value that covers it:</para>
    /// <list type="table">
    /// <listheader><term>Value</term><description>Use for</description></listheader>
    /// <item><term><see cref="BuildConfiguration.Any"/> (the default)</term>
    /// <description>whatever works the same in every build, using no server resources: formatting,
    /// calculations, metadata, the client data portal itself.</description></item>
    /// <item><term><see cref="BuildConfiguration.Backend"/></term>
    /// <description>the backend logic, wherever it runs: use case implementations and repositories. Registered on
    /// a server, in an ASP.NET server with a web UI, and in a thick client (which runs it locally); not in a thin
    /// client. The usual choice for anything that touches the database.</description></item>
    /// <item><term><see cref="BuildConfiguration.Server"/></term>
    /// <description>what only a server host does: the server data portal, background jobs, server health checks,
    /// anything that must not run (or start) in a thick client. Not for repositories: a thick client working locally
    /// would not have them.</description></item>
    /// <item><term><see cref="BuildConfiguration.Local"/></term>
    /// <description>what only a thick client needs to work locally: e.g. creating or migrating its local database,
    /// a local backup.</description></item>
    /// <item><term><see cref="BuildConfiguration.Client"/></term>
    /// <description>the UI side: services of the shell, forms and view models, session state. Registered in thin
    /// and thick clients and in the web UI of an ASP.NET server; not on a plain server.</description></item>
    /// </list>
    /// <para><b>One implementation per build.</b> An interface may have more than one implementation for different
    /// roles, as long as no app build registers two of them: a single-implementation service with two
    /// implementations in one build stops startup. Combinations that meet in a build:
    /// <see cref="BuildConfiguration.Client"/> with <see cref="BuildConfiguration.Server"/> (an ASP.NET server with
    /// a web UI); <see cref="BuildConfiguration.Client"/> with <see cref="BuildConfiguration.Local"/> or
    /// <see cref="BuildConfiguration.Backend"/> (a thick client); <see cref="BuildConfiguration.Any"/> with
    /// anything. Only <see cref="BuildConfiguration.Server"/> and <see cref="BuildConfiguration.Local"/> never meet.
    /// To give a thin client different implementations (e.g. repository stubs), put them in a separate assembly
    /// that only the thin client loads.</para>
    /// <para>A service whose interface allows multiple implementations (e.g. the data portal transports) is exempt:
    /// every implementation registered in the build is used.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class DefaultServiceImplementationAttribute : Attribute
    {
        /// <summary>
        /// Designates the class as the default implementation of a service interface.
        /// </summary>
        /// <param name="serviceInterfaceType">a type of the service interface implemented by the class</param>
        /// <param name="forBuildConfiguration">the roles of the app builds the implementation is meant for (see the
        /// remarks of <see cref="DefaultServiceImplementationAttribute"/>); <see cref="BuildConfiguration.Any"/> by
        /// default</param>
        public DefaultServiceImplementationAttribute(Type serviceInterfaceType,
            BuildConfiguration forBuildConfiguration = BuildConfiguration.Any)
        {
            ServiceInterfaceType = serviceInterfaceType ??
                throw new ArgumentNullException(nameof(serviceInterfaceType));
            ForBuildConfiguration = forBuildConfiguration;

            if (!serviceInterfaceType.IsInterface) throw new ArgumentException(
                $"Service interface type should be an interface while {serviceInterfaceType.FullName} is not.",
                nameof(serviceInterfaceType));
        }


        /// <summary>
        /// a type of the service interface implemented by the class
        /// </summary>
        public Type ServiceInterfaceType { get; }

        /// <summary>
        /// Gets the roles of the app builds the implementation is meant for: it is registered in a build that shares
        /// a role with it (see the remarks of <see cref="DefaultServiceImplementationAttribute"/>).
        /// </summary>
        public BuildConfiguration ForBuildConfiguration { get; }
    }
}
