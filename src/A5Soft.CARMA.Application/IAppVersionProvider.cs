using System;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Identifies the application. Implemented by the application itself, and registered on both the
    /// client and the server.
    /// </summary>
    /// <remarks>
    /// <para>The data portal uses it to refuse calls between a client and a server of different
    /// applications, or of versions that do not match (see <c>IDataPortalClientOptions.AppVersionMatch</c>):
    /// the server publishes its identity, and the client compares it with its own before the first call.</para>
    /// <para>There is deliberately no default implementation and no configuration setting: only the
    /// application knows what it is, and a value an administrator must update on every release would
    /// eventually be forgotten. A missing registration fails when the data portal is resolved.</para>
    /// <para>A typical implementation returns constants, or reads the version of the application's own
    /// assembly. A version with a suffix (e.g. <c>1.4.0-beta</c>) must be reduced to its numeric part.</para>
    /// </remarks>
    [Service(ServiceLifetime.Singleton)]
    public interface IAppVersionProvider
    {
        /// <summary>Gets the name of the application, the same on the client and the server.</summary>
        string AppName { get; }

        /// <summary>Gets the version of the application.</summary>
        Version AppVersion { get; }
    }
}
