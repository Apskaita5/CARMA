using A5Soft.CARMA.Application.Authorization.Authorizers;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.IoC.Plugins;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// The base of the app's <see cref="IMainMenuProvider"/>: the app builds its menu in <see cref="Build"/>, with its
    /// <see cref="Resources"/>, and places the plugins' group in it (<see cref="MenuBuilder.PluginsGroup"/>); each
    /// plugin of the scope's tenant adds one group to the plugins' group (<see cref="IPluginMenu"/>).
    /// </summary>
    /// <remarks>
    /// <para>Everything the menu needs is local: the plugins (the same on the client and the server), the identity
    /// (permissions, tenant, the tenant's plugins), the authorization rules and the use case metadata. Build the
    /// menu again when the scope changes (login, tenant); call <see cref="MainMenu.Refresh"/> when the identity
    /// changes within the scope, or the UI language changes.</para>
    /// <para>The plugins' group is wherever the app places it, at the top level or in one of its groups, and is
    /// there only if a plugin of the tenant has a menu. Its key, <see cref="PluginsGroupKey"/>, and the keys under it
    /// are reserved for the plugins. The plugins' own groups are sorted by their labels.</para>
    /// <para>A use case item is enabled if the identity may invoke the use case and, for a tenant scoped one, the
    /// scope has a tenant; a group if any of its items is enabled.</para>
    /// <para>Publish the keys of the app's groups as public constants of the derived class: the GUI app adds to a
    /// group by its key (<see cref="GuiMenuBuilder.In"/>).</para>
    /// </remarks>
    public abstract class MainMenuProviderBase : IMainMenuProvider
    {
        /// <summary>
        /// The key of the plugins' group, wherever the app places it (<see cref="MenuBuilder.PluginsGroup"/>).
        /// </summary>
        public const string PluginsGroupKey = "plugins";

        private readonly IAuthenticationStateProvider _authentication;
        private readonly IAuthorizationProvider _authorization;
        private readonly IMetadataProvider _metadata;
        private readonly PluginCatalog _plugins;


        /// <summary>
        /// Creates the provider.
        /// </summary>
        /// <param name="authentication">the scope's identity</param>
        /// <param name="authorization">the authorization rules of the use cases</param>
        /// <param name="metadata">the metadata provider, for the titles of the use cases</param>
        /// <param name="plugins">the installed plugins</param>
        protected MainMenuProviderBase(IAuthenticationStateProvider authentication, IAuthorizationProvider authorization,
            IMetadataProvider metadata, PluginCatalog plugins)
        {
            _authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
            _authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _plugins = plugins ?? throw new ArgumentNullException(nameof(plugins));
        }


        /// <summary>
        /// Gets the app's resources for the labels of its groups and of the plugins' group.
        /// </summary>
        protected abstract ResourceManager Resources { get; }

        /// <summary>
        /// Gets a value indicating whether the app is tenant based. If it is, the menu has the plugins assigned to the
        /// scope's tenant (<see cref="CarmaIdentity.TenantPlugins"/>), and none before a tenant is entered; if not,
        /// every installed plugin, for every user.
        /// </summary>
        protected abstract bool IsTenantBased { get; }


        /// <summary>
        /// Gets the key of a plugin's group (under <see cref="PluginsGroupKey"/>).
        /// </summary>
        /// <param name="pluginId">the plugin's id</param>
        public static string GetPluginGroupKey(string pluginId) => PluginsGroupKey + "." + pluginId;

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">a plugin of the tenant has a menu, but <see cref="Build"/> has not
        /// placed the plugins' group</exception>
        public MainMenu GetMenu()
        {
            var plugins = GetScopePlugins().Where(p => null != p.Menu).ToList();

            // the replacements first: every item is created with them applied
            var menu = new MainMenu(_metadata, GetReplacements(plugins), plugins, Authorize);

            Build(new MenuBuilder(menu, null, new MenuPart(GetType().Name, Resources, _metadata)));

            if (plugins.Count > 0 && !menu.PluginsPlaced) throw new InvalidOperationException(
                $"The tenant's plugins ({string.Join(", ", plugins.Select(p => p.Id))}) have menus, but " +
                $"{GetType().Name}.{nameof(Build)} has not placed the plugins' group: call " +
                $"{nameof(MenuBuilder.PluginsGroup)} where it belongs.");

            menu.TrimSeparators();
            menu.Refresh();

            return menu;
        }


        /// <summary>
        /// Builds the app's menu, placing the plugins' group in it (<see cref="MenuBuilder.PluginsGroup"/>) if any
        /// plugin can have a menu.
        /// </summary>
        /// <param name="menu">the builder</param>
        protected abstract void Build(MenuBuilder menu);


        private IEnumerable<PluginAssembly> GetScopePlugins()
        {
            if (!IsTenantBased) return _plugins.Plugins;

            var identity = _authentication.Identity;
            if (string.IsNullOrWhiteSpace(identity.TenantId)) return Enumerable.Empty<PluginAssembly>();

            var installed = _plugins.Plugins.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);

            return identity.TenantPlugins.Where(installed.ContainsKey).Select(id => installed[id]);
        }

        private static IReadOnlyDictionary<Type, Type> GetReplacements(IEnumerable<PluginAssembly> plugins)
        {
            var replacements = new Dictionary<Type, (Type UseCase, string PluginId)>();

            foreach (var plugin in plugins.Where(p => null != p.Menu))
            {
                foreach (var replacement in plugin.Menu.GetUseCaseReplacements() ?? new Dictionary<Type, Type>())
                {
                    MenuPart.EnsureUseCase(replacement.Key, "key");
                    MenuPart.EnsureUseCase(replacement.Value, "value");
                    if (replacement.Key.Assembly == plugin.Assembly || replacement.Value.Assembly != plugin.Assembly)
                        throw new InvalidOperationException($"Plugin {plugin.Id} can only replace an app use case " +
                            $"with its own ({replacement.Key.FullName} with {replacement.Value.FullName}).");
                    if (replacements.TryGetValue(replacement.Key, out var other)) throw new InvalidOperationException(
                        $"Plugins {other.PluginId} and {plugin.Id} both replace {replacement.Key.FullName}.");

                    replacements.Add(replacement.Key, (replacement.Value, plugin.Id));
                }
            }

            return replacements.ToDictionary(r => r.Key, r => r.Value.UseCase);
        }

        private void Authorize(MainMenu menu)
        {
            var identity = _authentication.Identity;
            var hasTenant = !string.IsNullOrWhiteSpace(identity.TenantId);

            foreach (var item in menu.AllItems().Where(i => i.ItemType == MenuItemType.UseCase))
            {
                item.IsEnabled = (hasTenant || !item.UseCaseType.IsDefined(typeof(TenantScopedAttribute), false))
                    && _authorization.GetAuthorizer(item.UseCaseType).CanInvoke(identity);
            }

            menu.UpdateGroupStates();
        }
    }
}
