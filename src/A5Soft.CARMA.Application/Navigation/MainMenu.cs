﻿using A5Soft.CARMA.Application.Authorization;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Application.Navigation
{
    [Serializable]
    public class MainMenu
    {

        public List<MenuItem> TopItems { get; } = new List<MenuItem>();


        /// <summary>
        /// Adds a top menu group.
        /// </summary>
        /// <param name="name">a name of the menu item (should be unique within main menu for group items)</param>
        /// <param name="displayName">a resource key string for <see cref="MenuItem.DisplayName"/></param>
        /// <param name="resourceType"><see cref="Type"/> that contains the resources
        /// for <see cref="MenuItem.DisplayName"/> and <see cref="MenuItem.Description"/></param>
        /// <param name="icon">Icon of the menu item if exists.</param>
        public MenuItem AddMainMenuTopGroup(string name, string displayName, Type resourceType, string icon = "")
        {
            var result = MenuItem.CreateMainMenuTopGroup(name, displayName, resourceType, icon);
            TopItems.Add(result);
            return result;
        }

        /// <summary>
        /// Adds a top leaf menu item.
        /// </summary>
        /// <param name="metadataProvider">a metadata provider for use case localized descriptions</param>
        /// <param name="icon">Icon of the menu item if exists.</param>
        /// <param name="useCaseType">A type of the (main) use case that handles action
        /// associated with the menu item. E.g. create invoice use case handles menu item "New Invoice".</param>
        public void AddMainMenuTopItem(IMetadataProvider metadataProvider, Type useCaseType, string icon = "")
        {
            TopItems.Add(MenuItem.CreateMainMenuTopItem(metadataProvider, useCaseType, icon));
        }

        /// <summary>
        /// Gets a menu group item (submenu) that has the name requested.
        /// </summary>
        /// <param name="name">a name of the menu group item (submenu) to find</param>
        /// <returns>a menu group item (submenu) that has the name requested; null if no such menu group</returns>
        public MenuItem GetMenuGroup(string name)
        {
            if (name.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(name));

            name = name.Trim();

            MenuItem result;
            foreach (var topItem in TopItems)
            {
                result = topItem.GetMenuGroup(name);
                if (!result.IsNull()) return result;
            }

            return null;
        }

        /// <summary>
        /// Resets group menus (submenus) enabled state after (if) a GUI app adds its own menu items.
        /// </summary>
        public void ResetEnabledForMenuGroups()
        {
            foreach (var item in TopItems)
            {
                item.ResetEnabledForMenuGroups();
            }
        }


        internal void SetAuthorization(IAuthorizationProvider authorizationProvider, ClaimsIdentity user)
        {
            foreach (var item in TopItems)
            {
                item.SetAuthorization(authorizationProvider, user);
            }
        }

        internal void ReplaceUseCases(Dictionary<Type, Type> useCaseInterfaceTypesToReplace)
        {
            foreach (var useCaseInterfaceTypeToReplace in useCaseInterfaceTypesToReplace)
            {
                foreach (var item in TopItems)
                {
                    item.ReplaceUseCase(useCaseInterfaceTypeToReplace.Key, 
                        useCaseInterfaceTypeToReplace.Value);
                }
            }
        }
    }
}
