using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds a max file size validation rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MaxFileSizeAttribute : PropertyValidationAttribute, IMaxFileSizeRuleMetadata
    {
        /// <summary>
        /// Max file size rule.
        /// </summary>
        /// <param name="maxSizeMB">max allowed file size (MB)</param>
        /// <param name="severity">a severity of the rule</param>
        public MaxFileSizeAttribute(int maxSizeMB, RuleSeverity severity = RuleSeverity.Error)
            : base(typeof(IMaxFileSizeRule), severity)
        {
            if (maxSizeMB < 1) throw new ArgumentOutOfRangeException(nameof(maxSizeMB),
                maxSizeMB, "Max file size must be a positive number of MB.");
            MaxSizeMB = maxSizeMB;
            NullIsValid = true;
        }


        /// <inheritdoc cref="IMaxFileSizeRuleMetadata.MaxSizeMB"/>
        public int MaxSizeMB { get; }
    }
}
