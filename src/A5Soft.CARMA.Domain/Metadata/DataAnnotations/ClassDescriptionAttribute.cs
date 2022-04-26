using System;
using System.Diagnostics.CodeAnalysis;

namespace A5Soft.CARMA.Domain.Metadata.DataAnnotations
{
    /// <summary>
    /// Allows to decorate a class with localizable values that could be used
    /// for entity (edit) form captions, headers and menu items for create a new instance action
    /// as well as help uri for the respective help file/resource topic.
    /// Can use it as is by setting ResourceType value in the attribute decorator
    /// or inherit this class and set ResourceType in the constructor.
    /// </summary>
    /// <remarks>Inherited class is obviously a different entity,
    /// therefore this attribute can only be used either on concrete (final) business entity (class)
    /// or on business entity interface used for business metadata
    /// (that inherits <see cref="IDomainObject"/>).</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, 
        AllowMultiple = false, Inherited = false)]
    public class ClassDescriptionAttribute : Attribute
    {
        #region Member Fields

        private Type _resourceType;
        private readonly LocalizableString _nameForNew = new LocalizableString(nameof(NameForNew));
        private readonly LocalizableString _nameForOld = new LocalizableString(nameof(NameForOld));
        private readonly LocalizableString _helpUri = new LocalizableString(nameof(HelpUri));

        #endregion

        #region All Constructors

        /// <summary>
        /// Default constructor for DisplayAttribute.  All associated string properties and methods will return <c>null</c>.
        /// </summary>
        public ClassDescriptionAttribute() { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the NameForNew attribute property, which may be a resource key string.
        /// <para>
        /// Consumers must use the <see cref="GetNameForNew"/> method to retrieve the UI display string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure a localized
        /// name for display.
        /// <para>
        /// The <see cref="GetNameForNew"/> method will return either the literal, non-localized
        /// string or the localized string when <see cref="ResourceType"/> has been specified.
        /// </para>
        /// </remarks>
        /// <value>
        /// NameForNew is generally used as a caption or header for an UI form bound to the member
        /// bearing this attribute.  A <c>null</c> or empty string is legal, and consumers must allow for that.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
        public string NameForNew
        {
            get
            {
                return this._nameForNew.Value;
            }
            set
            {
                if (this._nameForNew.Value != value)
                {
                    this._nameForNew.Value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the NameForOld attribute property, which may be a resource key string.
        /// <para>
        /// Consumers must use the <see cref="GetNameForOld"/> method to retrieve the UI display string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure a localized
        /// description for display.
        /// <para>
        /// The <see cref="GetNameForOld"/> method will return either the literal, non-localized
        /// string or the localized string when <see cref="ResourceType"/> has been specified.
        /// </para>
        /// </remarks>
        /// <value>
        /// NameForOld is generally used as a caption or header for an UI form bound to the member
        /// bearing this attribute.  A <c>null</c> or empty string is legal, and consumers must allow for that.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
        public string NameForOld
        {
            get
            {
                return this._nameForOld.Value;
            }
            set
            {
                if (this._nameForOld.Value != value)
                {
                    this._nameForOld.Value = value;
                }
            }
        }
                                                 
        /// <summary>
        /// Gets or sets the HelpUri attribute property, which may be a resource key string.
        /// <para>
        /// Consumers must use the <see cref="GetHelpUri"/> method to retrieve
        /// the URI to help file/resource topic.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure a localized
        /// description for display.
        /// <para>
        /// The <see cref="GetHelpUri"/> method will return either the literal, non-localized
        /// string or the localized string when <see cref="ResourceType"/> has been specified.
        /// </para>
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
        public string HelpUri
        {
            get
            {
                return this._helpUri.Value;
            }
            set
            {
                if (this._helpUri.Value != value)
                {
                    this._helpUri.Value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Type"/> that contains the resources for 
        /// <see cref="NameForNew"/>,, <see cref="NameForOld"/> and <see cref="NameForCreateNew"/>.
        /// Using <see cref="ResourceType"/> along with these Key properties, allows
        /// the <see cref="GetNameForNew"/>, <see cref="GetNameForOld"/>
        /// and <see cref="GetNameForCreateNew"/> methods to return localized values.
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

                    this._nameForNew.ResourceType = value;
                    this._nameForOld.ResourceType = value;
                }
            }
        }
                   
        #endregion

        #region Methods

        /// <summary>
        /// Gets the UI display string for NameForNew.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="NameForNew"/>
        /// or the localized string found when <see cref="ResourceType"/> has been specified
        /// and <see cref="NameForNew"/> represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="NameForNew"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="NameForNew"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// <para>
        /// Can return <c>null</c> and will not fall back onto other values, as it's more likely for the
        /// consumer to want to fall back onto the property name.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="NameForNew"/>
        /// property, but a public static property with a name matching the <see cref="NameForNew"/>
        /// value couldn't be found  on the <see cref="ResourceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
        public string GetNameForNew()
        {
            return this._nameForNew.GetLocalizableValue();
        }

        /// <summary>
        /// Gets the UI display string for NameForOld.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="NameForOld"/>
        /// or the localized string found when <see cref="ResourceType"/> has been specified
        /// and <see cref="NameForOld"/> represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="NameForOld"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="NameForOld"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="NameForOld"/>
        /// property, but a public static property with a name matching the <see cref="NameForOld"/>
        /// value couldn't be found on the <see cref="ResourceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
        public string GetNameForOld()
        {
            return this._nameForOld.GetLocalizableValue();
        }

        /// <summary>
        /// Gets a (localized) help URI for respective help file/resource topic about the entity.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="HelpUri"/>
        /// or the localized string found when <see cref="ResourceType"/> has been specified
        /// and <see cref="HelpUri"/> represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="HelpUri"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="HelpUri"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// <para>
        /// Can return <c>null</c> and will not fall back onto other values, as it's more likely for the
        /// consumer to want to fall back onto the property name.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="HelpUri"/>
        /// property, but a public static property with a name matching the <see cref="HelpUri"/>
        /// value couldn't be found  on the <see cref="ResourceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
        public string GetHelpUri()
        {
            return this._helpUri.GetLocalizableValue();
        }

        #endregion
    }
}
