using System;
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
        /// Gets a list of custom attributes of type TAttribute for the property specified.
        /// Includes attributes defined for the same property by the ancestor classes and interfaces.
        /// </summary>
        /// <typeparam name="TAttribute">a type of the custom attribute to get</typeparam>
        /// <param name="baseProp"></param>
        public static List<TAttribute> GetCustomAttributesWithInheritance<TAttribute>(
            this PropertyInfo baseProp) where TAttribute : Attribute
        {
            if (null == baseProp) throw new ArgumentNullException(nameof(baseProp));

            var result = new List<TAttribute>();

            foreach (var prop in GetInheritedProperties(baseProp))
            {
                var attributes = prop.GetCustomAttributes(typeof(TAttribute), false);
                if (null != attributes && attributes.Length > 0)
                    result.AddRange(attributes.Select(a => (TAttribute)a));
            }

            foreach (var prop in GetInterfaceProperties(baseProp))
            {
                var attributes = prop.GetCustomAttributes(typeof(TAttribute), false);
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
        /// <param name="baseProp"></param>
        public static TAttribute GetCustomAttributeWithInheritance<TAttribute>(
            this PropertyInfo baseProp) where TAttribute : Attribute
        {
            if (null == baseProp) throw new ArgumentNullException(nameof(baseProp));

            foreach (var prop in GetInheritedProperties(baseProp))
            {
                var result = prop.GetCustomAttributes(typeof(TAttribute), false);
                if (null != result && result.Length > 0) return (TAttribute)result[0];
            }

            foreach (var prop in GetInterfaceProperties(baseProp))
            {
                var result = prop.GetCustomAttributes(typeof(TAttribute), false);
                if (null != result && result.Length > 0) return (TAttribute)result[0];
            }

            return null;
        }

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
                    result.AddRange(attributes.Select(a => (TAttribute)a));
            }

            foreach (var interfaceType in GetAllInterfaces(type))
            {
                var attributes = interfaceType.GetCustomAttributes(typeof(TAttribute), false);
                if (null != attributes && attributes.Length > 0)
                    result.AddRange(attributes.Select(a => (TAttribute)a));
            }

            return result;
        }
               

        /// <summary>
        /// Gets a localized name of an enum value. (see <see cref="DisplayAttribute.Name"/>)
        /// </summary>
        /// <typeparam name="TEnum">type of enum (could be nullable)</typeparam>
        /// <param name="value"></param>
        public static string GetEnumDisplayName<TEnum>(this TEnum value)
        {
            return value.GetEnumDisplayProperty(a => a.GetName(), 
                v => value?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// Gets a localized short name of an enum value. (see <see cref="DisplayAttribute.ShortName"/>)
        /// </summary>
        /// <typeparam name="TEnum">type of enum (could be nullable)</typeparam>
        /// <param name="value"></param>
        public static string GetEnumDisplayShortName<TEnum>(this TEnum value)
        {
            return value.GetEnumDisplayProperty(a => a.GetShortName(),
                v => value?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// Gets a localized description of an enum value. (see <see cref="DisplayAttribute.Description"/>)
        /// </summary>
        /// <typeparam name="TEnum">type of enum (could be nullable)</typeparam>
        /// <param name="value"></param>
        public static string GetEnumDisplayDescription<TEnum>(this TEnum value)
        {
            return value.GetEnumDisplayProperty(a => a.GetDescription(),
                v => value?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// Gets a localized description of an enum value. (see <see cref="DisplayAttribute.Description"/>)
        /// </summary>
        /// <typeparam name="TEnum">type of enum (could be nullable)</typeparam>
        /// <param name="value"></param>
        public static string GetEnumDescription<TEnum>(TEnum value)
        {
            return value.GetEnumDisplayProperty(a => a.GetDescription(), v => string.Empty);
        }

        /// <summary>
        /// Gets a localized dataSource for the enum values.
        /// </summary>
        /// <typeparam name="TEnum">type of enum</typeparam>
        /// <param name="values"></param>
        public static List<(TEnum Value, string Name, string Description)> GetEnumDataSource<TEnum>(
            this IEnumerable<TEnum> values) where TEnum : struct
        {
            Type enumType = typeof(TEnum);
            if (!enumType.IsEnum) throw new InvalidOperationException(
                "Method GetEnumDisplayProperty is only applicable for Enum types.");

            var result = new List<(TEnum Value, string Name, string Description)>();
            foreach (var enumValue in values)
            {
                var fieldInfo = enumType.GetField(enumValue.ToString());

                var descriptionAttributes = fieldInfo.GetCustomAttributes(
                    typeof(DisplayAttribute), false) as DisplayAttribute[];

                if (null == descriptionAttributes || descriptionAttributes.Length < 1 ||
                    descriptionAttributes[0].Name.IsNullOrWhiteSpace())
                {
                    result.Add((Value: enumValue, Name: enumValue.ToString(), Description: string.Empty));
                }
                else
                {
                    result.Add((Value: enumValue, Name: descriptionAttributes[0].GetName() ?? enumValue.ToString(), 
                        Description: descriptionAttributes[0].GetDescription() ?? string.Empty));
                }
            }

            return result;
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
            var result = new List<PropertyInfo>(interfaceType.GetProperties().Where(p => p.Name == propName));

            if (null != interfaceType.BaseType) 
                result.AddRange(GetInterfaceProperties(interfaceType.BaseType, propName));

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
            if (type.IsInterface) return GetInheritedInterfaces(type);

            var result = new List<Type>();

            foreach (var interfaceType in type.GetInterfaces())
            {
                result.AddRange(GetInheritedInterfaces(interfaceType));
            }

            return result;
        }

        private static List<Type> GetInheritedInterfaces(Type interfaceType)
        {
            var result = new List<Type>(){ interfaceType };

            if (null != interfaceType.BaseType)
                result.AddRange(GetInheritedInterfaces(interfaceType.BaseType));

            return result;
        }

        private static string GetEnumDisplayProperty<TEnum>(this TEnum value, Func<DisplayAttribute, string> propGetter, 
            Func<TEnum, string> defaultValueGetter)
        {
            Type enumType = null;
            if (typeof(TEnum).IsEnum) enumType = typeof(TEnum);
            if (null == enumType) enumType = Nullable.GetUnderlyingType(typeof(TEnum));
            if (null == enumType || !enumType.IsEnum) throw new InvalidOperationException(
                "Method GetEnumDisplayProperty is only applicable for Enum types.");

            if (null == value) return defaultValueGetter(value);

            var fieldInfo = enumType.GetField(value.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(
                typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (null == descriptionAttributes || descriptionAttributes.Length < 1 ||
                propGetter(descriptionAttributes[0]).IsNullOrWhiteSpace())
                return defaultValueGetter(value);

            var result = propGetter(descriptionAttributes[0]);

            return result.IsNullOrWhiteSpace() ? defaultValueGetter(value) : result;
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

    }
}
