using A5Soft.CARMA.Application.DataPortal.EndPoints;

namespace A5Soft.CARMA.Application.DataPortal.Client
{
    /// <summary>
    /// The data portal endpoint configured for the app, e.g. for a Blazor Server app using a separate application
    /// server, or for a standalone app (local); none if the user chooses one at login.
    /// </summary>
    public interface IDataPortalConnectionConfiguration
    {
        /// <summary>Gets the endpoint of every scope the app does not set one for; null if none is configured.</summary>
        DataPortalEndpoint Endpoint { get; }
    }
}
