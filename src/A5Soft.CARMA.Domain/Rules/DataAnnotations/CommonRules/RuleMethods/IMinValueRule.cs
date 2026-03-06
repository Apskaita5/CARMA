namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// An interface for a min value validation rule.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IMinValueRule : IValidationRule
    {
    }
}
