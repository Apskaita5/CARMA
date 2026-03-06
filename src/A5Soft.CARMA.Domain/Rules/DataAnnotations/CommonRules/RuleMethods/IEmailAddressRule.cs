namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// An interface for an email address validation rule.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IEmailAddressRule : IValidationRule
    {
    }
}
