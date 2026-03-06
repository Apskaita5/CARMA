namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Base metadata for rules that references another property (e.g. greater than) extracted from validation attributes.
    /// </summary>
    /// <remarks>Rules with extra metadata shall define a descendant interface to accomodate extra fields.</remarks>
    public interface IRefPropertyRuleMetadata : IRuleMetadata
    {
        /// <summary>
        /// Gets a name of the other property to check.
        /// </summary>
        string ReferenceProperty { get; }
    }
}
