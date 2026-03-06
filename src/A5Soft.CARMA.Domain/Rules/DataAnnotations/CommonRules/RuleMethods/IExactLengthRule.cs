namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// An interface for an exact (string) length validation rule.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IExactLengthRule : IValidationRule
    {
    }
}
