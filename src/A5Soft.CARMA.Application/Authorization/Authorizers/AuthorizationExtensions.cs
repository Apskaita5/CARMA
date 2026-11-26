using A5Soft.CARMA.Application.Authorization.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Authorization.Authorizers
{
    /// <summary>
    /// Authorization of a use case invoked locally.
    /// </summary>
    public static class AuthorizationExtensions
    {
        /// <summary>
        /// Authorizes a local invocation of a use case: makes sure that the scope's identity is current, then checks it
        /// against the use case's rules. Call it on the local branch of every use case base; the server data portal
        /// authorizes remote calls.
        /// </summary>
        /// <exception cref="NotAuthenticatedException">not logged in, or the session has ended</exception>
        /// <exception cref="NotAuthorizedException">none of the permissions required</exception>
        public static async Task AuthorizeAsync(this IAuthenticationStateProvider authentication,
            IUseCaseAuthorizer authorizer, CancellationToken cancellationToken = default)
        {
            if (null == authentication) throw new ArgumentNullException(nameof(authentication));
            if (null == authorizer) throw new ArgumentNullException(nameof(authorizer));

            await authentication.EnsureCurrentAsync(cancellationToken).ConfigureAwait(false);
            authorizer.EnsureCanInvoke(authentication.Identity);
        }

        /// <summary>
        /// Gets a value indicating whether the scope's identity may invoke a use case: for the UI (menus, buttons).
        /// Does not refresh the identity.
        /// </summary>
        public static bool CanInvoke(this IAuthenticationStateProvider authentication,
            IAuthorizationProvider authorization, Type useCaseType)
        {
            if (null == authentication) throw new ArgumentNullException(nameof(authentication));
            if (null == authorization) throw new ArgumentNullException(nameof(authorization));
            return authorization.GetAuthorizer(useCaseType).CanInvoke(authentication.Identity);
        }
    }
}
