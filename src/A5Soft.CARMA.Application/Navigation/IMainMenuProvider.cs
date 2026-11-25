using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// Builds the main menu of the scope's user, locally: no data portal call. Implemented by the app, deriving from
    /// <see cref="MainMenuProviderBase"/> and marked <c>[DefaultServiceImplementation(typeof(IMainMenuProvider))]</c>.
    /// </summary>
    [Service(ServiceLifetime.Scoped)]
    public interface IMainMenuProvider
    {
        /// <summary>
        /// Builds the menu: the app's part, with the plugins' group (a group for each plugin of the scope's tenant)
        /// where the app places it, then the authorization of every item for the scope's identity.
        /// </summary>
        /// <returns>a new menu</returns>
        MainMenu GetMenu();
    }
}
