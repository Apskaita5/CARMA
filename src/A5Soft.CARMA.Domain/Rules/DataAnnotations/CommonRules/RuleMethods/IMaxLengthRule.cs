namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// An interface for a max string length validation rule.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IMaxLengthRule : IValidationRule
    {
    }
}
