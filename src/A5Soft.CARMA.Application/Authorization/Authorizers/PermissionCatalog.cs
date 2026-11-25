using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Application.Authorization.Authorizers
{
    /// <summary>
    /// Every permission of the app and its plugins: one instance per permission type, found by the
    /// startup scan. Built once by <see cref="IoC.IoCManagerBase"/>, which adds it to the container. Immutable,
    /// thread safe.
    /// </summary>
    /// <remarks>Use it to build a role editor: <see cref="Permissions"/> lists everything that can be granted, and
    /// <see cref="GetPluginId"/> tells which plugin a permission belongs to, so that a tenant's editor can show only
    /// the permissions of the tenant's plugins.</remarks>
    public sealed class PermissionCatalog
    {
        private readonly Dictionary<Type, Permission> _byType;
        private readonly Dictionary<Guid, Permission> _byId;
        private readonly Dictionary<Permission, string> _pluginIds;


        private PermissionCatalog(List<(Permission Permission, string PluginId)> permissions)
        {
            _byType = permissions.ToDictionary(p => p.Permission.GetType(), p => p.Permission);
            _byId = permissions.ToDictionary(p => p.Permission.Id, p => p.Permission);
            _pluginIds = permissions.ToDictionary(p => p.Permission, p => p.PluginId);
            Permissions = permissions.Select(p => p.Permission)
                .OrderBy(p => p.Order).ThenBy(p => p.GetType().FullName, StringComparer.Ordinal)
                .ToList();
        }


        /// <summary>Gets every permission, in display order.</summary>
        public IReadOnlyList<Permission> Permissions { get; }


        /// <summary>
        /// Gets the permission of the type given.
        /// </summary>
        /// <exception cref="InvalidOperationException">the type is not a permission found by the scan</exception>
        public Permission Get(Type permissionType)
        {
            if (null == permissionType) throw new ArgumentNullException(nameof(permissionType));
            return _byType.TryGetValue(permissionType, out var result) ? result : throw new InvalidOperationException(
                $"Type {permissionType.FullName} is not a permission of the app: it must be a concrete type deriving " +
                $"from {nameof(Permission)} in an app or plugin assembly.");
        }

        /// <summary>Gets the permission with the identifier given; null if there is none.</summary>
        public Permission Find(Guid id) => _byId.TryGetValue(id, out var result) ? result : null;

        /// <summary>Gets the identifier of the plugin a permission belongs to; null for the app's.</summary>
        public string GetPluginId(Permission permission)
        {
            if (null == permission) throw new ArgumentNullException(nameof(permission));
            return _pluginIds.TryGetValue(permission, out var result) ? result : null;
        }


        /// <summary>
        /// Creates a catalog from permission types: each one is instantiated once.
        /// </summary>
        /// <param name="types">the permission types, each with the identifier of its plugin (null for none)</param>
        /// <exception cref="InvalidOperationException">a type cannot be instantiated, or two permissions have the same
        /// identifier</exception>
        internal static PermissionCatalog Create(IEnumerable<(Type Type, string PluginId)> types)
        {
            var permissions = new List<(Permission Permission, string PluginId)>();
            var problems = new List<string>();

            foreach (var (type, pluginId) in types.GroupBy(t => t.Type).Select(g => g.First()))
            {
                Permission permission;
                try
                {
                    permission = (Permission)Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    problems.Add($"Permission {type.FullName} cannot be created (a public parameterless constructor " +
                        $"is required): {ex.GetBaseException().Message}");
                    continue;
                }

                if (permission.Id == Guid.Empty) problems.Add($"Permission {type.FullName} has an empty identifier.");
                else permissions.Add((permission, pluginId));
            }

            foreach (var duplicate in permissions.GroupBy(p => p.Permission.Id).Where(g => g.Count() > 1))
                problems.Add($"Permissions {string.Join(" and ", duplicate.Select(p => p.Permission.GetType().FullName))} " +
                    $"have the same identifier {duplicate.Key}.");

            if (problems.Count > 0) throw new InvalidOperationException(string.Join(" ", problems));

            return new PermissionCatalog(permissions);
        }
    }
}
