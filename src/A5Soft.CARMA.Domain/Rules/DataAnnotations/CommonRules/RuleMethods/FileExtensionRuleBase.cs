using A5Soft.CARMA.Domain.Files;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Linq;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Default implementation of a file extension validation rule.
    /// </summary>
    public abstract class FileExtensionRuleBase : PropertyValidationRuleBase<IFileExtensionRuleMetadata>,
        IFileExtensionRule
    {
        protected override string GetErrorDescription(object instance, object value, IEntityMetadata entityMetadata,
            IPropertyMetadata propertyMetadata, IFileExtensionRuleMetadata ruleMetadata)
        {
            if (propertyMetadata.PropertyType != typeof(string)) throw new ArgumentException(
                $"Cannot handle non string values ({propertyMetadata.Name}: {propertyMetadata.PropertyType.FullName}).",
                nameof(propertyMetadata));

            if (value is string extension)
            {
                if (extension.IsNullOrWhiteSpace()) return null;

                var fileType = extension.GetFileType();

                if (!ruleMetadata.AllowedTypes.Any(t => t == fileType)) return GetLocalizedErrorMessageFor(
                    extension, string.Join(", ", ruleMetadata.AllowedTypes.Select(t => t.ToString().ToLower())));
            }

            return null;
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="extension">a file extension that is invalid</param>
        /// <param name="validExtensions">valid file extensions</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string extension, string validExtensions);
    }
}
