using A5Soft.CARMA.Domain.Files;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for (allowed) file extension rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class FileExtensionAttributeBase : PropertyRuleAttributeBase
    {
        private readonly static Type[] _supportedValueTypes = new Type[] { typeof(string) };


        /// <summary>
        /// (allowed) file extension rule
        /// </summary>
        /// <param name="allowedTypes">a list of allowed file types</param>
        protected FileExtensionAttributeBase(params FileType[] allowedTypes) : base()
        {
            if (null == allowedTypes || allowedTypes.Length < 1) throw new ArgumentNullException(nameof(allowedTypes));

            AllowedTypes = allowedTypes;
        }


        /// <summary>
        /// Gets a list of allowed file types.
        /// </summary>
        public FileType[] AllowedTypes { get; set; }


        /// <inheritdoc/>
        protected override bool NullIsAlwaysValid => true;

        /// <inheritdoc/>
        protected override Type[] SupportedValueTypes => _supportedValueTypes;

        /// <inheritdoc/>
        protected override string GetErrorDescripton(object value, object entityInstance, Type entityType,
            string propertyDisplayName, Dictionary<string, (object Value, string DisplayName)> otherProperties)
        {
            var extension = (string)value;

            if (extension.IsNullOrWhiteSpace()) return null;

            var fileType = extension.GetFileType();

            if (!AllowedTypes.Any(t => t == fileType)) return GetLocalizedErrorMessageFor(
                extension, string.Join(", ", AllowedTypes.Select(t => t.ToString().ToLower())));

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
