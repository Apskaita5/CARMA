namespace A5Soft.CARMA.Domain.Rules
{
    /// <summary>
    /// Values for validation rule severities.
    /// </summary>
    public enum RuleSeverity
    {
        /// <summary>
        /// Represents a serious  business rule violation
        /// that should cause an object to be considered invalid.
        /// </summary>
        Error,

        /// <summary>
        /// Represents a business rule  violation that should be displayed to the user,
        /// yet should not make an object invalid.
        /// </summary>
        /// <remarks>Should cause dialog "Are you sure?"</remarks>
        Warning,

        /// <summary>
        /// Represents a business rule result that should be displayed to the user, but which is less
        /// severe than a warning.
        /// </summary>
        /// <remarks>Should NOT cause dialog "Are you sure?", only display field labels.</remarks>
        Information
    }
}
