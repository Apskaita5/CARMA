using System;

namespace A5Soft.CARMA.Domain.Metadata.DataAnnotations
{
    /// <summary>
    ///  a metadata description for use case (type) using DataAnnotations attributes
    /// </summary>
    public class UseCaseMetadata : IUseCaseMetadata
    {
        private readonly UseCaseDescriptionAttribute _displayAttribute = null;


        internal UseCaseMetadata(Type useCaseType) 
        {
            if (null == useCaseType) throw new ArgumentNullException(nameof(useCaseType));
            if ((!useCaseType.IsClass && !useCaseType.IsInterface) || useCaseType == typeof(string))
                throw new ArgumentException(
                "Metadata can only be defined for classes or interfaces.", nameof(useCaseType));

            UseCaseType = useCaseType;
            _displayAttribute = GetDescriptionAttribute(useCaseType);
        }


        /// <inheritdoc cref="IUseCaseMetadata.UseCaseType" />
        public Type UseCaseType { get; }

        /// <inheritdoc cref="IUseCaseMetadata.GetButtonTitle" />
        public string GetButtonTitle()
        {
            if (_displayAttribute.IsNull()) return UseCaseType.Name.SplitCamelCase();

            var value = _displayAttribute.GetButtonTitle();
            return value.IsNullOrWhiteSpace() ? UseCaseType.Name.SplitCamelCase() : value;
        }

        /// <inheritdoc cref="IUseCaseMetadata.GetConfirmationQuestion" />
        public string GetConfirmationQuestion()
        {
            var value = _displayAttribute?.GetConfirmationQuestion();
            return value ?? string.Empty;
        }

        /// <inheritdoc cref="IUseCaseMetadata.GetHelpUri" />
        public string GetHelpUri()
        {
            var value = _displayAttribute?.GetHelpUri();
            return value ?? string.Empty;
        }

        /// <inheritdoc cref="IUseCaseMetadata.GetSuccessMessage" />
        public string GetSuccessMessage()
        {
            var value = _displayAttribute?.GetSuccessMessage();
            return value ?? string.Empty;
        }


        private static UseCaseDescriptionAttribute GetDescriptionAttribute(Type useCaseType)
        {
            var displayAttributes = useCaseType.GetCustomAttributes(typeof(UseCaseDescriptionAttribute), false);
            if ((null == displayAttributes || displayAttributes.Length < 1) && useCaseType.IsClass)
            {
                foreach (Type useCaseInterface in useCaseType.GetInterfaces())
                {
                    displayAttributes = useCaseInterface.GetCustomAttributes(typeof(UseCaseDescriptionAttribute), false);
                    if (null != displayAttributes && displayAttributes.Length > 0) break;
                }
            }

            if (null != displayAttributes && displayAttributes.Length > 0)
                return (UseCaseDescriptionAttribute)displayAttributes[0];

            return null;
        }
    }
}
