using System;
using System.Diagnostics.CodeAnalysis;

namespace A5Soft.CARMA.Domain.Metadata.DataAnnotations
{
    /// <summary>
    /// Allows to decorate a class method with localizable values that could be used
    /// for action button tooltip.
    /// Can use it as is by setting ResourceType value in the attribute decorator
    /// or inherit this class and set ResourceType in the constructor.
    /// </summary>
    /// <remarks>Methods on business entities are used for (internal) business calculations
    /// only, as any interaction with "external world" is managed by use cases.
    /// Therefore method descriptions can only be applied to class methods (incl. inherited).
    /// They cannot be defined on business interfaces as their implementation
    /// are subject to the platform used (in rich entity models for winforms vs. javascript for web).</remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class MethodDescriptionAttribute : Attribute
    {
        #region Member Fields

        private Type _resourceType;
        private readonly LocalizableString _name = new LocalizableString(nameof(Name));

        #endregion

        #region All Constructors

        /// <summary>
        /// Default constructor for DisplayAttribute.  All associated string properties and methods will return <c>null</c>.
        /// </summary>
        public MethodDescriptionAttribute() { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Name attribute property, which may be a resource key string.
        /// <para>
        /// Consumers must use the <see cref="GetName"/> method to retrieve the UI display string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure a localized
        /// name for display.
        /// <para>
        /// The <see cref="GetName"/> method will return either the literal, non-localized
        /// string or the localized string when <see cref="ResourceType"/> has been specified.
        /// </para>
        /// </remarks>
        /// <value>
        /// Name is generally used as a tooltip for a button that invokes the method
        /// bearing this attribute.  A <c>null</c> or empty string is legal, and consumers must allow for that.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
        public string Name
        {
            get
            {
                return this._name.Value;
            }
            set
            {
                if (this._name.Value != value)
                {
                    this._name.Value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Type"/> that contains the resources for <see cref="Name"/>.
        /// Using <see cref="ResourceType"/> along with these Key properties, allows
        /// the <see cref="GetName"/> method to return localized values.
        /// </summary>
        public Type ResourceType
        {
            get
            {
                return this._resourceType;
            }
            set
            {
                if (this._resourceType != value)
                {
                    this._resourceType = value;

                    this._name.ResourceType = value;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the UI display string for Name.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="Name"/>
        /// or the localized string found when <see cref="ResourceType"/> has been specified
        /// and <see cref="Name"/> represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="Name"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="Name"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// <para>
        /// Can return <c>null</c> and will not fall back onto other values, as it's more likely for the
        /// consumer to want to fall back onto the property name.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="Name"/>
        /// property, but a public static property with a name matching the <see cref="Name"/>
        /// value couldn't be found  on the <see cref="ResourceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
        public string GetName()
        {
            return this._name.GetLocalizableValue();
        }

        #endregion
    }
}
