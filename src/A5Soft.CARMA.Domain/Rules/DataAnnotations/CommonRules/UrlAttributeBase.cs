using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for url value rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class UrlAttributeBase : PropertyRuleAttributeBase
    {
        private readonly static Type[] _supportedValueTypes = new Type[] { typeof(string) };


        /// <summary>
        /// url format rule
        /// </summary>
        protected UrlAttributeBase() : base() {}


        /// <inheritdoc/>
        protected override bool NullIsAlwaysValid => true;

        /// <inheritdoc/>
        protected override Type[] SupportedValueTypes => _supportedValueTypes;

        /// <inheritdoc/>
        protected override string GetErrorDescripton(object value, object entityInstance, Type entityType,
            string propertyDisplayName, Dictionary<string, (object Value, string DisplayName)> otherProperties)
        {
            var url = ((string)value)?.Trim();

            if (url.IsNullOrWhiteSpace() ||
                url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                || url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                || url.StartsWith("ftp://", StringComparison.InvariantCultureIgnoreCase)) return null;

            return GetLocalizedErrorMessageFor(propertyDisplayName);
        }

        /// <summary>
        /// Implement this method to get a localized error message for current culture.
        /// </summary>
        /// <param name="localizedPropName">a localized name of the property that is invalid</param>
        /// <returns>a localized error message for current culture</returns>
        protected abstract string GetLocalizedErrorMessageFor(string localizedPropName);
    }
}
