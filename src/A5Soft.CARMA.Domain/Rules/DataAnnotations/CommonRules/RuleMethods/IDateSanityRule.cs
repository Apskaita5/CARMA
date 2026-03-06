namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// An interface for a date sanity check rule.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IDateSanityRule : IValidationRule
    {
    }
}
