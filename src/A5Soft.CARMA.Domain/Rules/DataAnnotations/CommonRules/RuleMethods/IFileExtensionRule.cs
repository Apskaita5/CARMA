namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// An interface for a file extension validation rule.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IFileExtensionRule : IValidationRule
    {
    }
}
