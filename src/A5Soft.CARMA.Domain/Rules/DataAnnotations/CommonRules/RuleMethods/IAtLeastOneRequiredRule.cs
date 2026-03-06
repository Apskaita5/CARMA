namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// An interface for at least one value required rule.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IAtLeastOneRequiredRule : IValidationRule
    {
    }
}
