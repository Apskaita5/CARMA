namespace A5Soft.CARMA.Application.DataPortal.Client
{
    /// <summary>
    /// How strictly the client compares its application version with the server's.
    /// The application names must always be equal.
    /// </summary>
    public enum AppVersionMatch
    {
        /// <summary>The versions must be equal.</summary>
        Exact = 0,

        /// <summary>The major and minor versions must be equal; build and revision may differ, e.g. for a
        /// server-only hotfix that should not force every client to update.</summary>
        MajorMinor = 1,

        /// <summary>The versions are not compared; only the application names are.</summary>
        None = 2
    }
}
