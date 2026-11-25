using A5Soft.CARMA.Domain;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.Authorization.Authorizers
{
    /// <summary>
    /// A provider for a localized not authorized exception.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IAuthorizationExceptionProvider
    {
        /// <summary>
        /// Gets an app defined localized exception derived from <see cref="NotAuthorizedExceptionBase"/>
        /// that indicates that the user does not have certain permission.
        /// </summary>
        /// <param name="permissions">permissions that the use needs in order to invoke a use case</param>
        /// <param name="context">additional context if any</param>
        /// <returns>an app defined localized exception derived from <see cref="NotAuthorizedExceptionBase"/></returns>
        NotAuthorizedExceptionBase ExceptionForPermissions(IReadOnlyCollection<Permission> permissions, string context = null);

        /// <summary>
        /// Gets an app defined localized exception derived from <see cref="NotAuthorizedExceptionBase"/>
        /// that indicates that user is not an admin while the use case is only meant for admin users.
        /// </summary>
        /// <param name="context">additional context if any</param>
        /// <returns>an app defined localized exception derived from <see cref="NotAuthorizedExceptionBase"/></returns>
        NotAuthorizedExceptionBase ExceptionForAdmin(string context = null);
    }
}
