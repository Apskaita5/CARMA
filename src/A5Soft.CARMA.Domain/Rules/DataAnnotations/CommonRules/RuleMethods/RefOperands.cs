using A5Soft.CARMA.Domain.Metadata;
using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules.RuleMethods
{
    /// <summary>
    /// Operands of a reference property rule: validated and reference values
    /// normalized to the same (underlying) type.
    /// </summary>
    internal readonly struct RefOperands
    {
        public RefOperands(Type valueType, object value, object referenceValue, IPropertyMetadata referenceProperty)
        {
            ValueType = valueType;
            Value = value;
            ReferenceValue = referenceValue;
            ReferenceProperty = referenceProperty;
        }

        /// <summary>Underlying (non-nullable) type of both properties.</summary>
        public Type ValueType { get; }

        /// <summary>Validated property value (normalized).</summary>
        public object Value { get; }

        /// <summary>Reference property value (normalized).</summary>
        public object ReferenceValue { get; }

        /// <summary>Metadata of the reference property.</summary>
        public IPropertyMetadata ReferenceProperty { get; }
    }
}
