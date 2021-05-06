using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Metadata.DataAnnotations;

namespace A5Soft.CARMA.Domain.Test
{
    public class MockEntityMetadata : IEntityMetadata
    {
        public Type EntityType { get; }
        public ReadOnlyDictionary<string, IPropertyMetadata> Properties { get; } 
        public string[] PropertyNames { get; }
        public ReadOnlyDictionary<string, IMethodMetadata> Methods { get; } = new ReadOnlyDictionary<string, IMethodMetadata>(
            new Dictionary<string, IMethodMetadata>());


        public MockEntityMetadata(Type entityType)
        {
            EntityType = entityType;

            var props = new Dictionary<string, IPropertyMetadata>();
            foreach (var propertyInfo in EntityType.GetProperties()
                .Where(p => !p.GetCustomAttributes(typeof(IgnorePropertyMetadataAttribute), false).Any()))
            {
                props.Add(propertyInfo.Name, new MockPropertyMetadata(entityType, propertyInfo.Name, 
                    !propertyInfo.CanWrite, propertyInfo.PropertyType));
            }
            Properties = new ReadOnlyDictionary<string, IPropertyMetadata>(props);

            PropertyNames = props.Select(v => v.Key).ToArray();
        }

        public IPropertyMetadata GetPropertyMetadata(string propertyName)
        {
            if (Properties.ContainsKey(propertyName)) return Properties[propertyName];
            return null;
        }

        public string GetDisplayNameForCreateNew() => "Display Name For Create New";

        public string GetDisplayNameForNew() => "Display Name For New";

        public string GetDisplayNameForOld() => "Display Name For Old";

        public string GetHelpUri() => "";
    }
}
