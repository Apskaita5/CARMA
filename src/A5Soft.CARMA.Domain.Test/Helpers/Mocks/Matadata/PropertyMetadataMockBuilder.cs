using A5Soft.CARMA.Domain.Metadata;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Domain.Test.Helpers.Mocks.Matadata
{
    /// <summary>
    /// Fluent builder for creating IPropertyMetadata mocks.
    /// </summary>
    public class PropertyMetadataMockBuilder
    {
        private readonly Mock<IPropertyMetadata> _mock;
        private Type _entityType;
        private string _name;
        private bool _isReadOnly;
        private Type _propertyType;
        private string _displayName;
        private string _shortName;
        private string _description;
        private string _prompt;
        private string _groupName;
        private int _displayOrder = 10000;
        private bool _autoGenerate = true;
        private bool _autoGenerateFilter = true;
        private object _value;

        public PropertyMetadataMockBuilder(string propertyName, Type propertyType = null)
        {
            _mock = new Mock<IPropertyMetadata>();
            _name = propertyName;
            _propertyType = propertyType ?? typeof(string);
            _displayName = propertyName; // Default to property name
        }

        public PropertyMetadataMockBuilder ForEntityType(Type entityType)
        {
            _entityType = entityType;
            return this;
        }

        public PropertyMetadataMockBuilder ForEntityType<T>()
        {
            _entityType = typeof(T);
            return this;
        }

        public PropertyMetadataMockBuilder IsReadOnly(bool value = true)
        {
            _isReadOnly = value;
            return this;
        }

        public PropertyMetadataMockBuilder WithDisplayName(string displayName)
        {
            _displayName = displayName;
            return this;
        }

        public PropertyMetadataMockBuilder WithShortName(string shortName)
        {
            _shortName = shortName;
            return this;
        }

        public PropertyMetadataMockBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public PropertyMetadataMockBuilder WithPrompt(string prompt)
        {
            _prompt = prompt;
            return this;
        }

        public PropertyMetadataMockBuilder WithGroupName(string groupName)
        {
            _groupName = groupName;
            return this;
        }

        public PropertyMetadataMockBuilder WithDisplayOrder(int order)
        {
            _displayOrder = order;
            return this;
        }

        public PropertyMetadataMockBuilder WithAutoGenerate(bool autoGenerate)
        {
            _autoGenerate = autoGenerate;
            return this;
        }

        public PropertyMetadataMockBuilder WithAutoGenerateFilter(bool autoGenerateFilter)
        {
            _autoGenerateFilter = autoGenerateFilter;
            return this;
        }

        public PropertyMetadataMockBuilder WithValue(object value)
        {
            _value = value;
            return this;
        }

        public IPropertyMetadata Build()
        {
            _mock.Setup(x => x.EntityType).Returns(_entityType);
            _mock.Setup(x => x.Name).Returns(_name);
            _mock.Setup(x => x.IsReadOnly).Returns(_isReadOnly);
            _mock.Setup(x => x.PropertyType).Returns(_propertyType);
            _mock.Setup(x => x.GetDisplayName()).Returns(_displayName ?? _name);
            _mock.Setup(x => x.GetDisplayShortName()).Returns(_shortName ?? _displayName ?? _name);
            _mock.Setup(x => x.GetDisplayDescription()).Returns(_description ?? string.Empty);
            _mock.Setup(x => x.GetDisplayPrompt()).Returns(_prompt ?? string.Empty);
            _mock.Setup(x => x.GetDisplayGroupName()).Returns(_groupName ?? string.Empty);
            _mock.Setup(x => x.GetDisplayOrder()).Returns(_displayOrder);
            _mock.Setup(x => x.GetDisplayAutoGenerate(It.IsAny<bool>())).Returns(_autoGenerate);
            _mock.Setup(x => x.GetAutoGenerateFilter(It.IsAny<bool>())).Returns(_autoGenerateFilter);
            _mock.Setup(x => x.GetValue(It.IsAny<object>())).Returns(_value);

            return _mock.Object;
        }

        //public static implicit operator IPropertyMetadata(PropertyMetadataMockBuilder builder)
        //    => builder.Build();
    }
}
