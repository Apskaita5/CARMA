namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// What a <see cref="DataPortalResponse"/> holds.
    /// </summary>
    public enum DataPortalResultKind
    {
        /// <summary>No result: the remote method returns nothing, or it has returned null.</summary>
        None = 0,

        /// <summary>A result value, as JSON.</summary>
        Value = 1,

        /// <summary>A downloaded file.</summary>
        File = 2,

        /// <summary>The call has failed; see <see cref="DataPortalResponse.Error"/>.</summary>
        Error = 3
    }
}
