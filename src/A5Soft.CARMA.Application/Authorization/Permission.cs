using System;

namespace A5Soft.CARMA.Application.Authorization
{
    /// <summary>
    /// Abstracts a permission required for either base or plugin functionality and provides a localization point.
    /// Every actual permission is a descendant of this type, i.e. one type per permission, with a public parameterless
    /// constructor. Use cases list the permissions that grant the right to invoke them with
    /// <see cref="AuthorizeAttribute"/>; an identity holds the identifiers of the permissions granted
    /// (<see cref="CarmaIdentity.Permissions"/>).
    /// </summary>
    /// <remarks>Every permission type in the app's and the plugins' assemblies is found by the startup scan and kept in
    /// the <see cref="PermissionCatalog"/>; two permissions with the same <see cref="Id"/> stop startup. Read the
    /// localized properties on access (e.g. from resources), so that they follow the current UI culture.</remarks>
    public abstract class Permission
    {
        /// <summary>Gets the identifier of the permission: unique and never changed once released.</summary>
        public abstract Guid Id { get; }

        /// <summary>Gets a display order of the permission within the total list or a group (if any).</summary>
        public abstract int Order { get; }

        /// <summary>Gets a localized name of the permission for use in GUI.</summary>
        public abstract string Name { get; }

        /// <summary>Gets a localized group name of the permission for use in GUI.</summary>
        public abstract string GroupName { get; }

        /// <summary>Gets a localized description of the permission for use in GUI.</summary>
        public abstract string Description { get; }

        /// <summary>
        /// Gets a value indicating whether the permission is granted per tenant: it is only meaningful within a tenant,
        /// and a tenant administrator (<see cref="CarmaIdentity.IsTenantAdmin"/>) has it within the tenant.
        /// </summary>
        public abstract bool IsTenantScoped { get; }

        /// <inheritdoc />
        public override string ToString() => Name ?? GetType().Name;
    }
}
