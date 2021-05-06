using System;
using System.Collections.Generic;
using System.Text;
using A5Soft.CARMA.Domain.Metadata;

namespace A5Soft.CARMA.Domain.Test
{
    public class MockPropertyMetadata : IPropertyMetadata
    {
        public MockPropertyMetadata(Type entityType, string name, bool isReadOnly, Type propertyType)
        {
            EntityType = entityType;
            Name = name;
            IsReadOnly = isReadOnly;
            PropertyType = propertyType;
        }

        public Type EntityType { get; }
        public string Name { get; }
        public bool IsReadOnly { get; }
        public Type PropertyType { get; }

        public bool GetAutoGenerateFilter(bool defaultValue = true) => false;

        public bool GetDisplayAutoGenerate(bool defaultValue = true) => true;

        public string GetDisplayDescription() => $"{Name} description";

        public string GetDisplayGroupName() => $"{Name} group name";

        public string GetDisplayName() => $"{Name} display name";

        public int GetDisplayOrder() => 0;

        public string GetDisplayPrompt() => $"{Name} prompt";

        public string GetDisplayShortName() => $"{Name} short name";

        public object GetValue(object instance)
        {
            var propInfo = EntityType.GetProperty(Name);
            return propInfo.GetValue(instance);
        }
    }
}
