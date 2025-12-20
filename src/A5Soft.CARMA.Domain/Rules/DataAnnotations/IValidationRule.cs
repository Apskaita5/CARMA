namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Base interface for all validation rules that can be resolved via DI.
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>
        /// Returns a broken rule if the business rule is violated.
        /// </summary>
        BrokenRule GetValidationResult(IValidationContext context);
    }
}
