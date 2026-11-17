using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A container interface for app wide settings and services.
    /// </summary>
    /// <remarks>Inherit the interface and implement the container subject to the app service requirements.</remarks>
    public interface IApplicationContext : Domain.IApplicationContext
    {
        /// <summary>
        /// Metadata provider.
        /// </summary>
        public IAuthorizationProvider AuthorizationProvider { get; }

        /// <summary>
        /// Client data portal for remote use case invocations.
        /// </summary>
        public IClientDataPortal DataPortal { get; }
    }
}
