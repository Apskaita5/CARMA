using A5Soft.CARMA.Application.DataPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// A base use case for app main menu fetch operation.
    /// </summary>
    public abstract class FetchMainMenuUseCaseBase : IUseCase
    {
        private readonly IAuthenticationStateProvider _authenticationStateProvider;
        private readonly IClientDataPortal _dataPortal;
        protected readonly ILogger _logger;

        protected FetchMainMenuUseCaseBase(IAuthenticationStateProvider authenticationStateProvider, 
            IClientDataPortal dataPortal, IAuthorizationProvider authorizationProvider, ILogger logger)
        {
            _dataPortal = dataPortal ?? throw new ArgumentNullException(nameof(dataPortal));
            _authenticationStateProvider = authenticationStateProvider 
                ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
            AuthorizationProvider = authorizationProvider 
                ?? throw new ArgumentNullException(nameof(authorizationProvider));
            _logger = logger;
        }


        /// <summary>
        /// Gets a current user.
        /// </summary>
        public Task<ClaimsIdentity> GetIdentityAsync()
        {
            return _authenticationStateProvider.GetIdentityAsync();
        }

        /// <summary>
        /// Gets an authorization service.
        /// </summary>
        protected IAuthorizationProvider AuthorizationProvider { get; }


        /// <summary>
        /// Fetches a main menu for the current user. 
        /// </summary>
        /// <returns>a main menu for the current user</returns>
        public async Task<MainMenu> FetchAsync()
        {
            _logger?.LogMethodEntry(this.GetType(), nameof(FetchAsync));

            var identity = await GetIdentityAsync();

            if (null == identity || !identity.IsAuthenticated) return new MainMenu();

            MainMenu result;

            if (_dataPortal.IsRemote)
            {
                try
                {
                    result = (await _dataPortal.FetchAsync<MainMenu>(this.GetType(), identity)).Result;
                }
                catch (Exception e)
                {
                    _logger?.LogError(e);
                    throw;
                }

                _logger?.LogMethodExit(this.GetType(), nameof(FetchAsync), result);

                return result;
            }

            try
            {
                result = GetBaseMainMenu();

                var replacedUseCaseInterfaces = await GetReplacedUseCaseInterfaceTypesAsync();

                result.ReplaceUseCases(replacedUseCaseInterfaces);

                var plugins = await GetPluginMenuItemsAsync();

                foreach (var itemsCollection in plugins)
                {
                    foreach (var menuKey in itemsCollection
                        .Select(p => p.MenuGroupName.Trim().ToLowerInvariant()).Distinct())
                    {
                        var menuItem = result.GetMenuGroup(menuKey);
                        if (menuItem.IsNull()) throw new InvalidOperationException(
                            $"Plugin error - cannot find a base menu group (submenu) by name {menuKey}.");

                        menuItem.AddSeparator();
                        foreach (var item in itemsCollection
                            .Where(p => p.MenuGroupName.Trim().Equals(menuKey, 
                                StringComparison.OrdinalIgnoreCase))
                            .Select(p => p.MenuItem))
                        {
                            menuItem.AddPluginMenuItem(item);
                        }
                    }
                }

                result.SetAuthorization(AuthorizationProvider, identity);
            }
            catch (Exception e)
            {
                _logger?.LogError(e);
                throw;
            }

            _logger?.LogMethodExit(this.GetType(), nameof(FetchAsync), result);

            return result;
        }

        /// <summary>
        /// Implement this method to create a base app menu, i.e. ignoring plugins.
        /// </summary>
        /// <returns>a <see cref="MainMenu"/> for base app functionality</returns>
        /// <remarks>This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract MainMenu GetBaseMainMenu();

        /// <summary>
        /// Implement this method to fetch a list of menu items for each relevant plugin.
        /// </summary>
        /// <remarks>This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task<List<List<(string MenuGroupName, MenuItem MenuItem)>>> GetPluginMenuItemsAsync();

        /// <summary>
        /// Implement this method to fetch a dictionary of replaced use case interfaces for each relevant plugin.
        /// </summary>
        /// <remarks>This method is always executed on server side (if data portal is configured).
        /// Plugins may wish to change how the base menu behaves,
        /// e.g. to create a new invoice using plugin use case instead of the base one.</remarks>
        protected abstract Task<Dictionary<Type, Type>> GetReplacedUseCaseInterfaceTypesAsync();

    }
}
