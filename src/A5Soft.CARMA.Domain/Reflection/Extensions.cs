using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace A5Soft.CARMA.Domain.Reflection
{
    /// <summary>
    /// Extensions for common reflection tasks.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets a list of custom attributes of type TAttribute for the (class or interface) type specified.
        /// Includes attributes defined by the ancestor classes and interfaces.
        /// </summary>
        /// <typeparam name="TAttribute">a type of the custom attribute to get</typeparam>
        /// <param name="type"></param>
        public static List<TAttribute> GetCustomAttributesWithInheritance<TAttribute>(
            this Type type) where TAttribute : Attribute
        {
            if (null == type) throw new ArgumentNullException(nameof(type));

            var result = new List<TAttribute>();

            foreach (var inheritedType in GetInheritedTypes(type))
            {
                var attributes = inheritedType.GetCustomAttributes(typeof(TAttribute), false);
                if (null != attributes && attributes.Length > 0)
                    result.AddRange(attributes.Cast<TAttribute>());
            }

            foreach (var interfaceType in GetAllInterfaces(type))
            {
                var attributes = interfaceType.GetCustomAttributes(typeof(TAttribute), false);
                if (null != attributes && attributes.Length > 0)
                    result.AddRange(attributes.Cast<TAttribute>());
            }

            return result;
        }

        /// <summary>
        /// Gets a list of custom attributes of type TAttribute for the property specified.
        /// Includes attributes defined for the same property by the ancestor classes and interfaces.
        /// </summary>
        /// <typeparam name="TAttribute">a type of the custom attribute to get</typeparam>
        /// <param name="prop"></param>
        public static List<TAttribute> GetCustomAttributesWithInheritance<TAttribute>(
            this PropertyInfo prop) where TAttribute : Attribute
        {
            if (null == prop) throw new ArgumentNullException(nameof(prop));

            var result = new List<TAttribute>();

            var selfAttributes = prop.GetCustomAttributes(typeof(TAttribute), false);
            if (null != selfAttributes && selfAttributes.Length > 0)
                result.AddRange(selfAttributes.Cast<TAttribute>());

            foreach (var iProp in GetInheritedProperties(prop))
            {
                var attributes = iProp.GetCustomAttributes(typeof(TAttribute), false);
                if (null != attributes && attributes.Length > 0)
                    result.AddRange(attributes.Select(a => (TAttribute)a));
            }

            foreach (var iProp in GetInterfaceProperties(prop))
            {
                var attributes = iProp.GetCustomAttributes(typeof(TAttribute), false);
                if (null != attributes && attributes.Length > 0)
                    result.AddRange(attributes.Select(a => (TAttribute)a));
            }

            return result;
        }

        /// <summary>
        /// Gets a first custom attribute of type TAttribute for the property specified.
        /// Includes attributes defined for the same property by the ancestor classes and interfaces.
        /// </summary>
        /// <typeparam name="TAttribute">a type of the custom attribute to get</typeparam>
        /// <param name="prop"></param>
        /// <remarks>First checks attribute on the prop itself, then inherited prop and finaly interface prop</remarks>
        public static TAttribute GetCustomAttributeWithInheritance<TAttribute>(
            this PropertyInfo prop) where TAttribute : Attribute
        {
            if (null == prop) throw new ArgumentNullException(nameof(prop));

            var selfAttributes = prop.GetCustomAttributes(typeof(TAttribute), false);
            if (null != selfAttributes && selfAttributes.Length > 0)
                return (TAttribute)selfAttributes[0];

            foreach (var iProp in GetInheritedProperties(prop))
            {
                var result = iProp.GetCustomAttributes(typeof(TAttribute), false);
                if (null != result && result.Length > 0) return (TAttribute)result[0];
            }

            foreach (var iProp in GetInterfaceProperties(prop))
            {
                var result = iProp.GetCustomAttributes(typeof(TAttribute), false);
                if (null != result && result.Length > 0) return (TAttribute)result[0];
            }

            return null;
        }

        /// <summary>
        /// Gets public instance properties for a class or interface.
        /// Solves GetProperties() for interfaces problem.
        /// </summary>
        /// <typeparam name="T">a type to get public properties for</typeparam>
        /// <returns>public instance properties for a class or interface</returns>
        public static PropertyInfo[] GetPublicProperties<T>()
            where T : class
        {
            return typeof(T).GetPublicProperties();
        }

        /// <summary>
        /// Gets public instance properties for a class or interface.
        /// Solves GetProperties() for interfaces problem.
        /// </summary>
        /// <typeparam name="T">a type to get public properties for</typeparam>
        /// <param name="filter">a property filter method</param>
        /// <returns>public instance properties for a class or interface</returns>
        public static PropertyInfo[] GetPublicProperties<T>(Func<PropertyInfo, bool> filter)
            where T : class
        {
            return typeof(T).GetPublicProperties(filter);
        }

        /// <summary>
        /// Gets public instance properties for a class or interface.
        /// Solves GetProperties() for interfaces problem.
        /// </summary>
        /// <param name="forType">a type to get public properties for</param>
        /// <returns>public instance properties for a class or interface</returns>
        public static PropertyInfo[] GetPublicProperties(this Type forType)
        {
            if (null == forType) throw new ArgumentNullException(nameof(forType));

            if (forType.IsClass)
            {
                return forType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            }

            if (forType.IsInterface)
            {
                var result = new Dictionary<string, PropertyInfo>(
                    StringComparer.OrdinalIgnoreCase);
                foreach (var prop in forType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    result.Add(prop.Name, prop);
                }

                foreach (var implementedInterface in forType.GetInterfaces())
                {
                    foreach (var prop in implementedInterface.GetProperties(
                        BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (!result.ContainsKey(prop.Name)) result.Add(prop.Name, prop);
                    }
                }

                return result.Select(e => e.Value).ToArray();
            }

            throw new InvalidOperationException(
                $"Method {nameof(GetPublicProperties)} can only handle classes and interfaces while {forType.FullName} is not.");
        }

        /// <summary>
        /// Gets public instance properties for a class or interface.
        /// Solves GetProperties() for interfaces problem.
        /// </summary>
        /// <param name="forType">a type to get public properties for</param>
        /// <param name="filter">a property filter method</param>
        /// <returns>public instance properties for a class or interface</returns>
        public static PropertyInfo[] GetPublicProperties(this Type forType,
            Func<PropertyInfo, bool> filter)
        {
            if (null == forType) throw new ArgumentNullException(nameof(forType));
            if (null == filter) throw new ArgumentNullException(nameof(filter));

            if (forType.IsClass)
            {
                return forType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => filter(p)).ToArray();
            }

            if (forType.IsInterface)
            {
                var result = new Dictionary<string, PropertyInfo>(
                    StringComparer.OrdinalIgnoreCase);
                foreach (var prop in forType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => filter(p)))
                {
                    result.Add(prop.Name, prop);
                }

                foreach (var implementedInterface in forType.GetInterfaces())
                {
                    foreach (var prop in implementedInterface.GetProperties(
                        BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => filter(p)))
                    {
                        if (!result.ContainsKey(prop.Name)) result.Add(prop.Name, prop);
                    }
                }

                return result.Select(e => e.Value).ToArray();
            }

            throw new InvalidOperationException(
                $"Method {nameof(GetPublicProperties)} can only handle classes and interfaces while {forType.FullName} is not.");
        }


        private static List<PropertyInfo> GetInheritedProperties(PropertyInfo prop)
        {
            var result = new List<PropertyInfo>() { prop };
            if (null != prop.DeclaringType?.BaseType)
            {
                foreach (var inheritedProp in prop.DeclaringType.BaseType.GetProperties()
                    .Where(p => p.Name == prop.Name))
                {
                    result.AddRange(GetInheritedProperties(inheritedProp));
                }
            }
            return result;
        }

        private static List<PropertyInfo> GetInterfaceProperties(PropertyInfo prop)
        {
            var result = new List<PropertyInfo>();

            if (null != prop.DeclaringType && !prop.DeclaringType.IsInterface)
            {
                foreach (var implInterface in prop.DeclaringType.GetInterfaces())
                {
                    result.AddRange(GetInterfaceProperties(implInterface, prop.Name));
                }
            }

            return result;
        }

        private static List<PropertyInfo> GetInterfaceProperties(Type interfaceType, string propName)
        {
            var result = new List<PropertyInfo>(interfaceType.GetProperties()
                .Where(p => p.Name == propName));

            if (null != interfaceType.BaseType)
                result.AddRange(GetInterfaceProperties(interfaceType.BaseType, propName));

            foreach (var inheritedInterface in interfaceType.GetInterfaces())
            {
                result.AddRange(GetInterfaceProperties(inheritedInterface, propName));
            }

            return result;
        }

        private static List<Type> GetInheritedTypes(Type type)
        {
            var result = new List<Type>() { type };
            if (null != type.BaseType)
            {
                result.AddRange(GetInheritedTypes(type.BaseType));
            }
            return result;
        }

        private static List<Type> GetAllInterfaces(Type type)
        {
            var result = new List<Type>();

            if (type.IsInterface) result.Add(type);

            foreach (var interfaceType in type.GetInterfaces())
            {
                result.Add(interfaceType);
            }

            return result.Distinct().ToList();
        }

        private static List<Type> GetInheritedInterfaces(Type interfaceType)
        {
            var result = new List<Type> { interfaceType };
            foreach (var inheritedInterface in interfaceType.GetInterfaces())
            {
                result.AddRange(GetInheritedInterfaces(inheritedInterface));
            }
            return result;
        }

        private static string LookupResource(Type resourceManagerProvider, string resourceKey)
        {
            foreach (PropertyInfo staticProperty in resourceManagerProvider.GetProperties(
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (staticProperty.PropertyType == typeof(System.Resources.ResourceManager))
                {
                    var resourceManager = (System.Resources.ResourceManager)staticProperty.GetValue(null, null);
                    return resourceManager.GetString(resourceKey);
                }
            }

            return resourceKey; // Fallback with the key name
        }


        #region Enum Localization

        /// <summary>
        /// Gets a localized name of an enum value. (see <see cref="DisplayAttribute.Name"/>)
        /// </summary>
        /// <typeparam name="TEnum">type of enum (could be nullable)</typeparam>
        /// <param name="value"></param>
        public static string GetEnumDisplayName<TEnum>(this TEnum value)
        {
            if (value == null) return string.Empty;

            var metadata = GetEnumMetadata<TEnum>();
            return GetDisplayValue(metadata, value, m => m.GetDisplayName());
        }

        /// <summary>
        /// Gets a localized short name of an enum value. (see <see cref="DisplayAttribute.ShortName"/>)
        /// </summary>
        /// <typeparam name="TEnum">type of enum (could be nullable)</typeparam>
        /// <param name="value"></param>
        public static string GetEnumDisplayShortName<TEnum>(this TEnum value)
        {
            if (value == null) return string.Empty;

            var metadata = GetEnumMetadata<TEnum>();
            return GetDisplayValue(metadata, value, m => m.GetShortName());
        }

        /// <summary>
        /// Gets a localized description of an enum value. (see <see cref="DisplayAttribute.Description"/>)
        /// </summary>
        /// <typeparam name="TEnum">type of enum (could be nullable)</typeparam>
        /// <param name="value"></param>
        public static string GetEnumDisplayDescription<TEnum>(this TEnum value)
        {
            if (value == null) return string.Empty;

            var metadata = GetEnumMetadata<TEnum>();
            return GetDisplayValue(metadata, value, m => m.GetDescription()) ?? string.Empty;
        }


        private static readonly ConcurrentDictionary<Type, EnumMetadata> _enumCache =
            new ConcurrentDictionary<Type, EnumMetadata>();

        private static EnumMetadata GetEnumMetadata<TEnum>()
        {
            var enumType = typeof(TEnum);
            var underlyingType = Nullable.GetUnderlyingType(enumType) ?? enumType;

            if (null != underlyingType && underlyingType.IsEnum)
                return _enumCache.GetOrAdd(underlyingType, type => new EnumMetadata(type));

            if (enumType.IsEnum) return _enumCache.GetOrAdd(enumType, type => new EnumMetadata(type));

            throw new ArgumentException($"Type {enumType.Name} is not an enum type.");
        }

        private static string GetDisplayValue<TEnum>(EnumMetadata metadata, TEnum value, Func<EnumMemberMetadata, string> selector)
        {
            if (metadata.IsFlags) return GetFlagsDisplayValue(metadata, value, selector);
            return GetSingleDisplayValue(metadata, value, selector);
        }

        private static string GetSingleDisplayValue<TEnum>(EnumMetadata metadata, TEnum value,
            Func<EnumMemberMetadata, string> selector)
        {
            var valueName = value.ToString();

            if (metadata.Members.TryGetValue(valueName, out var memberMetadata))
            {
                return selector(memberMetadata) ?? valueName;
            }

            return valueName;
        }

        private static string GetFlagsDisplayValue<TEnum>(EnumMetadata metadata, TEnum value,
            Func<EnumMemberMetadata, string> selector)
        {
            var enumValue = Convert.ToInt64(value);
            var parts = new List<string>();

            // Handle zero value
            if (enumValue == 0)
            {
                var zeroMember = metadata.Members.Values.FirstOrDefault(m => m.Value == 0);
                if (zeroMember != null)
                {
                    return selector(zeroMember) ?? zeroMember.Name;
                }
                return value.ToString();
            }

            // Check each flag
            foreach (var member in metadata.Members.Values)
            {
                if (member.Value == 0)
                    continue;

                if ((enumValue & member.Value) == member.Value)
                {
                    var displayValue = selector(member) ?? member.Name;
                    parts.Add(displayValue);
                }
            }

            return parts.Count > 0 ? string.Join(", ", parts) : value.ToString();
        }

        private class EnumMetadata
        {
            public bool IsFlags { get; }
            public ConcurrentDictionary<string, EnumMemberMetadata> Members { get; }

            public EnumMetadata(Type enumType)
            {
                IsFlags = enumType.GetCustomAttribute<FlagsAttribute>() != null;
                Members = new ConcurrentDictionary<string, EnumMemberMetadata>();

                foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    var memberMetadata = new EnumMemberMetadata(field);

                    Members[field.Name] = memberMetadata;
                }
            }
        }

        private class EnumMemberMetadata
        {
            public EnumMemberMetadata(FieldInfo forField)
            {
                Name = forField.Name;
                Value = Convert.ToInt64(forField.GetValue(null));

                var descriptionAttributes = forField.GetCustomAttributes(
                    typeof(DisplayAttribute), false) as DisplayAttribute[];

                if (null == descriptionAttributes || descriptionAttributes.Length < 1)
                    DisplayAttribute = null;
                else DisplayAttribute = descriptionAttributes[0];
            }


            public long Value { get; }

            public string Name { get; }

            public DisplayAttribute DisplayAttribute { get; }


            public string GetDisplayName()
            {
                var result = DisplayAttribute?.GetName();
                if (!string.IsNullOrWhiteSpace(result)) return result;
                return Name;
            }

            public string GetShortName()
            {
                var result = DisplayAttribute?.GetShortName();
                if (!string.IsNullOrWhiteSpace(result)) return result;
                return Name;
            }

            public string GetDescription()
            {
                return DisplayAttribute?.GetDescription() ?? string.Empty;
            }
        }

        #endregion
    }
}
