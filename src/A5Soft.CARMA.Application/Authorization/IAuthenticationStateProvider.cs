using A5Soft.CARMA.Domain;
using System.Security.Claims;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Authorization
{
    /// <summary>
    /// interface for a service that manages current user state
    /// </summary>                      
    [Service(ServiceLifetime.Scoped)]
    public interface IAuthenticationStateProvider
    {
        /// <summary>
        /// Asynchronously gets a <see cref="ClaimsIdentity"/> for the current user.
        /// </summary>
        /// <returns>A task that, when resolved, gives a <see cref="ClaimsIdentity"/> for the current user.</returns>
        Task<ClaimsIdentity> GetIdentityAsync();

        /// <summary>
        /// Raises the <see cref="IdentityChanged"/> event.
        /// </summary>
        /// <param name="updatedIdentity">An updated <see cref="ClaimsIdentity"/>.</param>
        Task NotifyIdentityChangedAsync(ClaimsIdentity updatedIdentity);

        /// <summary>
        /// An event that provides notification when the <see cref="ClaimsIdentity"/>
        /// has changed. For example, this event may be raised if a user logs in or out.
        /// </summary>
        event IdentityChangedHandler IdentityChanged;
    }

    /// <summary>
    /// A handler for the <see cref="AuthenticationStateProviderBase.IdentityChanged"/> event.
    /// </summary>
    /// <param name="task">A <see cref="Task"/> that supplies the updated <see cref="ClaimsIdentity"/>.</param>
    public delegate void IdentityChangedHandler(Task<ClaimsIdentity> task);

}
