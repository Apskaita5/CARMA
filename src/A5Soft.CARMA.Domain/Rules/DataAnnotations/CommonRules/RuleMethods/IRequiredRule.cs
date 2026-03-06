namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// An interface for a required rule.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IRequiredRule : IValidationRule
    {
    }
}
