using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for url value rule.
    /// Can use it as is by setting ErrorMessageResourceType and ErrorMessageResourceName values
    /// in the attribute decorator or inherit this class and set ErrorMessageResourceType and ErrorMessageResourceName
    /// in the constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class UrlAttribute : System.ComponentModel.DataAnnotations.DataTypeAttribute
    {

        /// <summary>
        /// Gets or sets a value indicating severity of broken rule.
        /// </summary>
        public RuleSeverity Severity { get; set; }


        public UrlAttribute() : base(System.ComponentModel.DataAnnotations.DataType.Url)
        { }


        /// <inheritdoc cref="System.ComponentModel.DataAnnotations.ValidationAttribute.IsValid" />
        public override bool IsValid(object value)
        {
            if (Severity != RuleSeverity.Error) return true;
            return IsValidInternal(value as string);
        }

        /// <inheritdoc cref="IPropertyValidationAttribute.GetValidationResult" />
        public BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo,
            IEnumerable<IPropertyMetadata> relatedProps)
        {
            if (IsValidInternal(propInfo.GetValue(instance) as string)) return null;

            return new BrokenRule(this.GetType().FullName, propInfo.Name, 
                string.Format(CultureInfo.CurrentCulture, this.ErrorMessageString, 
                    propInfo.GetDisplayName()), Severity);
        }


        private bool IsValidInternal(string value)
        {
            if (null == value) return true;
            value = value.Trim();
            return value.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                   || value.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                   || value.StartsWith("ftp://", StringComparison.InvariantCultureIgnoreCase);
        }

    }
}
