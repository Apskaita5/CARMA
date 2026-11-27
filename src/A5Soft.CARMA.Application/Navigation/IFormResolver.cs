using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// Tells which form opens a use case for the scope: the GUI navigates through it, from the menu, the actions and
    /// any link, and creates the form type it gives.
    /// </summary>
    /// <remarks>
    /// <para>The form of a plugin that applies to the scope comes first (<see cref="IoC.Plugins.PluginCatalog.GetScopePlugins"/>),
    /// then the app's.</para>
    /// <para>Scoped, and up to date with the scope's identity (e.g. after entering a tenant).</para>
    /// </remarks>
    [Service(ServiceLifetime.Scoped)]
    public interface IFormResolver
    {
        /// <summary>
        /// Gets the form that opens a use case for the scope.
        /// </summary>
        /// <param name="useCaseType">the use case interface</param>
        /// <param name="dialog">whether the dialog (rather than the page or window)</param>
        /// <returns>the form type; null if there is none</returns>
        /// <exception cref="InvalidOperationException">two plugins of the scope's tenant serve the use case</exception>
        Type GetForm(Type useCaseType, bool dialog = false);

        /// <summary>
        /// Gets whether the use case has a form for the scope, e.g. to hide an item whose use case cannot be opened.
        /// </summary>
        /// <param name="useCaseType">the use case interface</param>
        /// <param name="dialog">whether the dialog (rather than the page or window)</param>
        bool HasForm(Type useCaseType, bool dialog = false);
    }
}
