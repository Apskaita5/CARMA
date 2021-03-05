using System;
using System.Diagnostics.CodeAnalysis;
using A5Soft.CARMA.Domain.Metadata.DataAnnotations;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Used to designate a use case interface that should be added to IoC container.
    /// Sets localizable name and description of the use case interface.
    /// Can use it as is by setting ResourceType value in the attribute decorator
    /// or inherit this class and set ResourceType in the constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class UseCaseAttribute : RemoteServiceAttribute
    {

        #region Member Fields

        private readonly LocalizableString _name = new LocalizableString("Name");
        private readonly LocalizableString _description = new LocalizableString("Description");

        #endregion

        #region All Constructors

        /// <summary>
        /// Default constructor for UseCaseAttribute.
        /// </summary>
        /// <param name="name">the Name attribute property, which may be a resource key string</param>
        /// <param name="description">the Description attribute property, which may be a resource key string</param>
        /// <param name="resourceType">the <see cref="System.Type"/> that contains the resources for
        /// <see cref="Name"/> and <see cref="Description"/></param>
        /// <param name="lifetime">a lifetime of the use case within an IoC container</param>
        /// <param name="defaultImplementation">a default implementation of the interface (if any) to
        /// use in IoC container unless specified otherwise</param>
        public UseCaseAttribute(string name, string description, Type resourceType, 
            ServiceLifetime lifetime = ServiceLifetime.Transient,
            Type defaultImplementation = null, params Type[] requiredLookups) : base(lifetime, defaultImplementation)
        {
            LookupTypes = requiredLookups;
            _name.Value = name;
            _description.Value = description;
            ResourceType = resourceType;

            _name.ResourceType = resourceType;
            _description.ResourceType = resourceType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Name attribute property, which may be a resource key string.
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
        /// The name is generally used as the field label for a UI element bound to the member
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
        /// Gets the Description attribute property, which may be a resource key string.
        /// <para>
        /// Consumers must use the <see cref="GetDescription"/> method to retrieve the UI display string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure a localized
        /// description for display.
        /// <para>
        /// The <see cref="GetDescription"/> method will return either the literal, non-localized
        /// string or the localized string when <see cref="ResourceType"/> has been specified.
        /// </para>
        /// </remarks>
        /// <value>
        /// Description is generally used as a tool tip or description a UI element bound to the member
        /// bearing this attribute.  A <c>null</c> or empty string is legal, and consumers must allow for that.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
        public string Description
        {
            get
            {
                return this._description.Value;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Type"/> that contains the resources for <see cref="Name"/>
        /// and <see cref="Description"/>.
        /// Using <see cref="ResourceType"/> along with these Key properties, allows the <see cref="GetName"/>
        /// and <see cref="GetDescription"/> methods to return localized values.
        /// </summary>
        public Type ResourceType { get; }

        /// <summary>
        /// Gets types of lookup values that the use case can request.
        /// </summary>
        public Type[] LookupTypes { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the UI display string for Name.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="Name"/> or the
        /// localized string found when <see cref="ResourceType"/> has been specified and <see cref="Name"/>
        /// represents a resource key within that resource type.
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
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="Name"/> property,
        /// but a public static property with a name matching the <see cref="Name"/> value couldn't be found
        /// on the <see cref="ResourceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
        public string GetName()
        {
            return this._name.GetLocalizableValue();
        }

        /// <summary>
        /// Gets the UI display string for Description.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="Description"/> or the
        /// localized string found when <see cref="ResourceType"/> has been specified and <see cref="Description"/>
        /// represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="Description"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="Description"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="Description"/> property,
        /// but a public static property with a name matching the <see cref="Description"/> value couldn't be found
        /// on the <see cref="ResourceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
        public string GetDescription()
        {
            return this._description.GetLocalizableValue();
        }

        #endregion

    }
}
