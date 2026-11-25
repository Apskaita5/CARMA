using A5Soft.CARMA.Application.Authorization.Identity;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.Authorization.Authorizers
{
    /// <summary>
    /// Decides who may invoke a use case, from its interface's <see cref="AuthorizeAttribute"/>,
    /// <see cref="AllowAnonymousAttribute"/> or <see cref="AllowSecondFactorPendingAttribute"/>.
    /// </summary>
    public interface IUseCaseAuthorizer
    {
        /// <summary>Gets the use case interface the rules are read from.</summary>
        Type UseCaseType { get; }

        /// <summary>Gets a value indicating whether anyone may invoke the use case.</summary>
        bool AllowsAnonymous { get; }

        /// <summary>
        /// Gets a value indicating whether a user who has not provided the second factor yet may invoke the use case.
        /// </summary>
        bool AllowsSecondFactorPending { get; }

        /// <summary>Gets a value indicating only app level admins may invoke the use case.</summary>
        bool AllowsAdminOnly { get; }

        /// <summary>
        /// Gets the permissions, any of which grants the right to invoke the use case; empty for any authenticated user.
        /// </summary>
        IReadOnlyList<Permission> Permissions { get; }

        /// <summary>Gets a value indicating whether the identity may invoke the use case.</summary>
        /// <param name="identity">the identity; null is treated as anonymous</param>
        /// <returns>true if the identity may invoke the use case</returns>
        bool CanInvoke(CarmaIdentity identity);

        /// <summary>Throws if the identity may not invoke the use case.</summary>
        /// <param name="identity">the identity; null is treated as anonymous</param>
        /// <exception cref="NotAuthenticatedException">the use case requires logging in (completely)</exception>
        /// <exception cref="NotAuthorizedExceptionBase">the identity holds none of the permissions</exception>
        /// <exception cref="InvalidOperationException">the use case interface declares no authorization at all</exception>
        void EnsureCanInvoke(CarmaIdentity identity);
    }
}