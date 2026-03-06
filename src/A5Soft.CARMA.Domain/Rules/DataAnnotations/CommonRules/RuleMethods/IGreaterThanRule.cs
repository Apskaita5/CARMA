namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// An interface for greater than (other property) rule.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IGreaterThanRule : IValidationRule
    {
    }
}
