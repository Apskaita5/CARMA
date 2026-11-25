namespace A5Soft.CARMA.Application.Authorization.Login
{
    /// <summary>
    /// Who must provide a second factor at login.
    /// </summary>
    public enum SecondFactorPolicy
    {
        /// <summary>Nobody.</summary>
        None,

        /// <summary>The users the app's <see cref="ISecondFactorProvider.IsRequiredAsync"/> says.</summary>
        PerUser,

        /// <summary>Administrators and tenant administrators, and the users the provider says.</summary>
        Admins,

        /// <summary>Every user.</summary>
        All
    }
}
