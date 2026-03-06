namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// An interface for a max file size validation rule.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IMaxFileSizeRule : IValidationRule
    {
    }
}
