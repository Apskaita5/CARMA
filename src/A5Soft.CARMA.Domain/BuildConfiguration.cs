using System;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// Application build configurations type (client/server).
    /// </summary>
    [Flags]
    public enum BuildConfiguration
    {
        /// <summary>
        /// service implementation should only be injected for client app
        /// </summary>
        Client = 1,

        /// <summary>
        /// service implementation should only be injected for server app
        /// </summary>
        Server = 2,

        /// <summary>
        /// service implementation should be injected for any app build configuration
        /// </summary>
        Any = 3
    }
}
