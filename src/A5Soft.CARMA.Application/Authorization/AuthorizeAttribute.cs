using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.Authorization
{
    /// <summary>
    /// Lists the permissions that grant the right to invoke a use case: holding any one of them is enough. Without
    /// permission types, any authenticated user may invoke it. Applied to the use case interface.
    /// </summary>
    /// <remarks>Every remote use case interface carries this attribute, <see cref="AllowAnonymousAttribute"/> or
    /// <see cref="AllowSecondFactorPendingAttribute"/>; a use case with none of them is refused to everyone (and
    /// reported by the data portal conventions). The attribute is read from the interface only, so that an
    /// implementation, e.g. a plugin's, cannot weaken it.</remarks>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class AuthorizeAttribute : Attribute
    {
        /// <summary>
        /// Creates the attribute.
        /// </summary>
        /// <param name="permissionTypes">the permission types (deriving from <see cref="Permission"/>), any of which
        /// grants the right to invoke the use case; none for any authenticated user</param>
        public AuthorizeAttribute(params Type[] permissionTypes)
            => PermissionTypes = permissionTypes ?? Type.EmptyTypes;

        /// <summary>Gets the permission types, any of which grants the right to invoke the use case.</summary>
        public IReadOnlyList<Type> PermissionTypes { get; }
    }
}
