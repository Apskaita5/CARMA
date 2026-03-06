namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// An interface for a min string length validation rule.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IMinLengthRule : IValidationRule
    {
    }
}
