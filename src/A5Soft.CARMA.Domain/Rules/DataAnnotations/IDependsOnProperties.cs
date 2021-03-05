using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// Should be used with ValidationAttribute if the validation of the property depends on other properties.
    /// </summary>
    public interface IDependsOnProperties
    {
        /// <summary>
        /// Gets names of the properties that the validation depends on.
        /// </summary>
        List<string> DependsOnProperties { get; }
    }
}
