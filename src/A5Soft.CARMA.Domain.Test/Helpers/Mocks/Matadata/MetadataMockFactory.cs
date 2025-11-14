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
        /// Creates entity metadata with common properties for a typical business entity.
        /// </summary>
        public static IEntityMetadata CreateTypicalEntityMetadata<T>(
            params string[] propertyNames)
        {
            var builder = new EntityMetadataMockBuilder(typeof(T));

            foreach (var propName in propertyNames)
            {
                builder.WithProperty(propName);
            }

            return builder.Build();
        }

        /// <summary>
        /// Creates a complete entity metadata for a user entity.
        /// </summary>
        public static IEntityMetadata CreateUserEntityMetadata()
        {
            return new EntityMetadataMockBuilder(typeof(object))
                .WithDisplayNameForNew("Create New User")
                .WithDisplayNameForOld("Edit User")
                .WithProperty("Name", b => b
                    .WithDisplayName("Full Name")
                    .WithPrompt("Enter user's full name")
                    .WithDisplayOrder(1))
                .WithProperty("Email", b => b
                    .WithDisplayName("Email Address")
                    .WithPrompt("user@example.com")
                    .WithDisplayOrder(2))
                .WithProperty("Age", b => b
                    .WithDisplayName("Age")
                    .WithDisplayOrder(3))
                .WithProperty("IsActive", b => b
                    .WithDisplayName("Active")
                    .WithDescription("Indicates if user account is active")
                    .WithDisplayOrder(4))
                .WithMethod("Save", m => m
                    .WithDisplayName("Save User"))
                .WithMethod("Delete", m => m
                    .WithDisplayName("Delete User"))
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

        /// <summary>
        /// Creates a metadata provider with no metadata (returns nulls).
        /// </summary>
        public static IMetadataProvider CreateEmptyMetadataProvider()
        {
            return new MetadataProviderMockBuilder().Build();
        }

        /// <summary>
        /// Creates a metadata provider for testing with typical entity.
        /// </summary>
        public static IMetadataProvider CreateTypicalMetadataProvider<TEntity>()
        {
            return new MetadataProviderMockBuilder()
                .WithEntityMetadata<TEntity>(builder => builder
                    .WithProperty("Id")
                    .WithProperty("Name")
                    .WithProperty("CreatedAt")
                    .WithProperty("UpdatedAt"))
                .Build();
        }
    }
}
