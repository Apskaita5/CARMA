using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// The roles of an app build, and the roles a service implementation (or a configuration, an external service,
    /// an unmanaged library) is meant for. An implementation is used by a build if they share a role.
    /// </summary>
    /// <remarks>
    /// <para>An app is built as one of four deployments:</para>
    /// <list type="bullet">
    /// <item><see cref="Server"/>: a server, reached only through the data portal;</item>
    /// <item><see cref="Client"/> | <see cref="Server"/>: an ASP.NET server that also serves a web UI (e.g. Blazor
    /// Server): the UI runs the use cases in process, the data portal serves remote clients;</item>
    /// <item><see cref="Client"/>: a thin client (e.g. Blazor WebAssembly), working through a remote server only;</item>
    /// <item><see cref="Client"/> | <see cref="Local"/>: a thick client (e.g. WinForms, WPF), working through a remote
    /// server or locally, running the use cases in process (e.g. on SQLite repositories).</item>
    /// </list>
    /// <para>An implementation is marked with the roles it serves: e.g. <see cref="Backend"/> for a repository or a
    /// use case implementation (a server, or a thick client working locally), <see cref="Server"/> for what only a
    /// server host does (a background job, a server health check), <see cref="Client"/> for the UI side.</para>
    /// </remarks>
    [Flags]
    public enum BuildConfiguration
    {
        /// <summary>The UI side: thin clients, thick clients, and the web UI of an ASP.NET server.</summary>
        Client = 1,

        /// <summary>A server host: the server data portal, background jobs, server health checks.</summary>
        Server = 2,

        /// <summary>The backend running inside a client process: a thick client working locally.</summary>
        Local = 4,

        /// <summary>The backend logic, wherever it runs: on a server, or locally in a thick client (use case
        /// implementations, repositories).</summary>
        Backend = Server | Local,

        /// <summary>Every build.</summary>
        Any = Client | Server | Local
    }
}
