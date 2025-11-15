using A5Soft.CARMA.Domain.Metadata;
using System;

namespace A5Soft.CARMA.Domain.Test.Helpers.Mocks.Matadata
{
    /// <summary>
    /// Factory for creating common metadata mocks.
    /// </summary>
    public static class MetadataMockFactory
    {
        /// <summary>
        /// Creates a minimal property metadata mock with just a name.
        /// </summary>
        public static IPropertyMetadata CreateMinimalPropertyMetadata(string propertyName)
        {
            return new PropertyMetadataMockBuilder(propertyName).Build();
        }

        /// <summary>
        /// Creates a typical string property metadata.
        /// </summary>
        public static IPropertyMetadata CreateStringPropertyMetadata(
            string propertyName,
            string displayName = null,
            bool isRequired = false)
        {
            var builder = new PropertyMetadataMockBuilder(propertyName, typeof(string))
                .WithDisplayName(displayName ?? propertyName)
                .WithPrompt($"Enter {displayName ?? propertyName}");

            if (isRequired)
                builder.WithDescription("Required field");

            return builder.Build();
        }

        /// <summary>
        /// Creates a typical numeric property metadata.
        /// </summary>
        public static IPropertyMetadata CreateNumericPropertyMetadata(
            string propertyName,
            Type numericType,
            string displayName = null)
        {
            return new PropertyMetadataMockBuilder(propertyName, numericType)
                .WithDisplayName(displayName ?? propertyName)
                .Build();
        }

        /// <summary>
        /// Creates a read-only property metadata.
        /// </summary>
        public static IPropertyMetadata CreateReadOnlyPropertyMetadata(
            string propertyName,
            string displayName = null)
        {
            return new PropertyMetadataMockBuilder(propertyName)
                .WithDisplayName(displayName ?? propertyName)
                .IsReadOnly(true)
                .Build();
        }

        /// <summary>
        /// Creates use case metadata for a CRUD operation.
        /// </summary>
        public static IUseCaseMetadata CreateCrudUseCaseMetadata(
            string operation,
            string entityName)
        {
            return new UseCaseMetadataMockBuilder(typeof(object))
                .WithButtonTitle($"{operation} {entityName}")
                .WithMenuTitle($"{operation} {entityName}")
                .WithConfirmationQuestion(
                    operation == "Delete"
                        ? $"Are you sure you want to delete this {entityName}?"
                        : null)
                .WithSuccessMessage($"{entityName} {operation.ToLower()}d successfully")
                .Build();
        }
    }
}
