using System;

namespace A5Soft.CARMA.Domain.Metadata
{
    /// <summary>
    /// Common interface for use case (class or interface) metadata localization provider.
    /// </summary>
    /// <remarks>Use case (method) could be commonly described in the respective buttons
    /// (as button title or tooltip for action icons).
    /// Use case (method) is also commonly associated with a conformation question and/or 
    /// success message.</remarks>
    public interface IUseCaseMetadata
    {
        /// <summary>
        /// Gets the type of the use case that the metadata is for.
        /// </summary>
        Type UseCaseType { get; }

        /// <summary>
        /// Gets a localized value that is used for display a button title (or a tooltip for action icons) 
        /// for the use case in the UI.
        /// </summary>
        /// <returns>a localized value that is used for display in the UI</returns>
        string GetButtonTitle();

        /// <summary>
        /// Gets a localized value that is used for display a (main or context) menu title for the use case in the UI.
        /// </summary>
        /// <returns>a localized value that is used for display in the UI</returns>
        string GetMenuTitle();

        /// <summary>
        /// Gets a localized value that is used to display a confirm dialog (before the use case is invoked).
        /// </summary>
        /// <returns>a localized value that is used to display a confirm dialog (before the use case is invoked)</returns>
        string GetConfirmationQuestion();

        /// <summary>
        /// Gets a localized value that is used to display a success message
        /// (to be shown after the use case is successfuly invoked).
        /// </summary>
        /// <returns>a localized value that is used to display a success message</returns>
        string GetSuccessMessage();

        /// <summary>
        /// Gets an URI for help file and topic for the use case.
        /// </summary>
        string GetHelpUri();
    }
}
