using System;

namespace A5Soft.CARMA.Domain.Metadata.DataAnnotations
{
    /// <summary>
    /// Default implementation of IMethodMetadata using DisplayAttribute.
    /// </summary>
    internal class MethodMetadata : IMethodMetadata
    {
        private readonly MethodDescriptionAttribute _displayAttribute;


        public MethodMetadata(Type entityType, string methodName, 
            MethodDescriptionAttribute displayAttribute)
        {
            if (null == entityType) throw new ArgumentNullException(nameof(entityType));
            if (methodName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(methodName));

            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            _displayAttribute = displayAttribute ?? throw new ArgumentNullException(nameof(displayAttribute));
            Name = methodName;
        }


        /// <inheritdoc cref="IMethodMetadata.EntityType" />
        public Type EntityType { get; }

        /// <inheritdoc cref="IMethodMetadata.Name" />
        public string Name { get; }


        /// <inheritdoc cref="IMethodMetadata.GetDisplayName" />
        public string GetDisplayName()
        {
            if (_displayAttribute.IsNull()) return Name.SplitCamelCase();

            var value = _displayAttribute.GetName();
            return value.IsNullOrWhiteSpace() ? Name.SplitCamelCase() : value;
        }

    }
}
