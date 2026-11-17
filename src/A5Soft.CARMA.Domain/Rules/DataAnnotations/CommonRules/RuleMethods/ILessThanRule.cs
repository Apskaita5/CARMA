namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// An interface for less than (other property) rule.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface ILessThanRule : IValidationRule
    {
    }
}
