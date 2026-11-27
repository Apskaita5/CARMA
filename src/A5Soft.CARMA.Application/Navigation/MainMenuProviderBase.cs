using A5Soft.CARMA.Application.Authorization.Authorizers;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.IoC.Plugins;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Linq;
using System.Resources;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// The base of the app's <see cref="IMainMenuProvider"/>: the app builds its menu in <see cref="Build"/>, with its
    /// <see cref="Resources"/>, and places the plugins' group in it (<see cref="MenuBuilder.PluginsGroup"/>); each
    /// plugin of the scope's tenant adds its own group to the plugins' group, and its items to the app's groups open to
    /// plugins (<see cref="IPluginMenu"/>).
    /// </summary>
    /// <remarks>
    /// <para>Everything the menu needs is local: the plugins (the same on the client and the server), the identity
    /// (permissions, tenant, the tenant's plugins), the authorization rules and the use case metadata. Build the
    /// menu again when the scope changes (login, tenant); call <see cref="MainMenu.Refresh"/> when the identity
    /// changes within the scope, or the UI language changes.</para>
    /// <para>The plugins are those that apply to the scope (<see cref="PluginCatalog.GetScopePlugins"/>): the
    /// tenant's in a tenant based app, every installed one otherwise.</para>
    /// <para>The plugins' group is wherever the app places it, at the top level or in one of its groups, and is
    /// there only if a plugin of the tenant has a group of its own with a use case item. Its key,
    /// <see cref="PluginsGroupKey"/>, and the keys under it are reserved for the plugins. The plugins' own groups are
    /// sorted by their labels.</para>
    /// <para>A group the app creates with openToPlugins (e.g. Reports) takes the plugins' own items after the app's,
    /// each plugin's after a separator, the plugins in the order of their labels.</para>
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
        /// Gets the key of a plugin's group (under <see cref="PluginsGroupKey"/>).
        /// </summary>
        /// <param name="pluginId">the plugin's id</param>
        public static string GetPluginGroupKey(string pluginId) => PluginsGroupKey + "." + pluginId;

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">a plugin of the tenant has a group of its own, but
        /// <see cref="Build"/> has not placed the plugins' group; or a plugin extends a group of the app that does not
        /// exist or is not open to plugins</exception>
        public MainMenu GetMenu()
        {
            var plugins = _plugins.GetScopePlugins(_authentication.Identity).Where(p => null != p.Menu).ToList();

            // the plugins' own groups first, so that the app's PluginsGroup can place them
            var menu = new MainMenu(_metadata, plugins, Authorize);
            menu.BuildPluginGroups();

            Build(new MenuBuilder(menu, null, new MenuPart(GetType().Name, Resources, _metadata, isApp: true)));

            if (menu.HasPluginGroups && !menu.PluginsPlaced) throw new InvalidOperationException(
                $"The tenant's plugins ({string.Join(", ", plugins.Select(p => p.Id))}) have groups of their own, but " +
                $"{GetType().Name}.{nameof(Build)} has not placed the plugins' group: call " +
                $"{nameof(MenuBuilder.PluginsGroup)} where it belongs.");

            // then their items in the app's groups open to plugins, after the app's items
            menu.ApplyPluginExtensions();

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
