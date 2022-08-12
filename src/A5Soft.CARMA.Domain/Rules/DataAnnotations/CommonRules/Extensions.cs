using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using A5Soft.CARMA.Domain.Reflection;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// Extension methods for data annotation attributes.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets a localized display name for the property if it is defined by <see cref="DisplayAttribute"/>
        /// (either directly for the property or by the ancestors or interfaces). Returns property name
        /// if no <see cref="DisplayAttribute"/> defined.
        /// </summary>
        /// <param name="validationContext">validation context</param>
        /// <param name="otherPropertyName">a name of the other (not <see cref="ValidationContext.MemberName"/>)
        /// property to get a localized display name for</param>
        /// <returns>a localized display name for the property or property name
        /// if no <see cref="DisplayAttribute"/> defined</returns>
        /// <exception cref="ArgumentNullException"><paramref name="validationContext"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="validationContext"/> has no
        /// <see cref="ValidationContext.ObjectType"/> set</exception>
        /// <exception cref="InvalidOperationException"><paramref name="otherPropertyName"/> is null or empty and
        /// <paramref name="validationContext"/> has no <see cref="ValidationContext.MemberName"/> set</exception>
        /// <exception cref="InvalidOperationException">no such property for the type</exception>
        public static string GetPropertyDisplayName(this ValidationContext validationContext, string otherPropertyName = null)
        {
            if (null == validationContext) throw new ArgumentNullException(nameof(validationContext));
            if (null == validationContext.ObjectType) throw new InvalidOperationException(
                "Validation context has no ObjectType set.");

            if (otherPropertyName.IsNullOrWhiteSpace()) otherPropertyName = validationContext.MemberName;
            if (otherPropertyName.IsNullOrWhiteSpace()) throw new InvalidOperationException(
                "Validation context has no MemberName set.");

            var prop = validationContext.ObjectType.GetProperty(otherPropertyName);
            if (null == prop) throw new InvalidOperationException(
                $"No property {otherPropertyName} on type {validationContext.ObjectType.FullName}.");

            var displayAttr = prop.GetCustomAttributeWithInheritance<DisplayAttribute>();

            if (null == displayAttr) return otherPropertyName;

            return displayAttr.GetName();
        }
    }
}
