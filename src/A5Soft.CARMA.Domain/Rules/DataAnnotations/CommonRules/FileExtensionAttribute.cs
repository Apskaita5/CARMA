using A5Soft.CARMA.Domain.Files;
using A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// An attribute that adds a file extension validation rule.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public abstract class FileExtensionAttribute : PropertyValidationAttribute, IFileExtensionRuleMetadata
    {
        private readonly static Type[] _supportedValueTypes = new Type[] { typeof(string) };


        /// <summary>
        /// (allowed) file extension rule
        /// </summary>
        /// <param name="allowedTypes">a list of allowed file types</param>
        protected FileExtensionAttribute(params FileType[] allowedTypes)
            : base(typeof(IFileExtensionRule), RuleSeverity.Error)
        {
            if (null == allowedTypes || allowedTypes.Length < 1) throw new ArgumentNullException(nameof(allowedTypes));

            AllowedTypes = allowedTypes;
            NullIsValid = true;
        }


        /// <inheritdoc cref="IFileExtensionRuleMetadata.AllowedTypes"/>
        public FileType[] AllowedTypes { get; set; }


        protected override void AddRelatedProperties(HashSet<string> relatedProperties)
        {
            base.AddRelatedProperties(relatedProperties);
        }
    }
}
