using System;
using System.Diagnostics.CodeAnalysis;

namespace A5Soft.CARMA.Domain.Metadata.DataAnnotations
{
    /// <summary>
    /// Allows to decorate a use case class (or interface) with localizable values that could be used
    /// for button title, confirmation question, success message
    /// as well as help uri for the respective help file/resource topic.
    /// Can use it as is by setting ResourceType value in the attribute decorator
    /// or inherit this class and set ResourceType in the constructor.
    /// </summary>
    /// <remarks>Inherited class is obviously a different entity,
    /// therefore this attribute can only be used either on concrete (final)
    /// use case (class) or on use case interface (used for business metadata).</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface,
        AllowMultiple = false, Inherited = false)]
    public class UseCaseDescriptionAttribute : Attribute
    {
        #region Member Fields

        private Type _resourceType;
        private readonly LocalizableString _buttonTitle = new LocalizableString(nameof(ButtonTitle));
        private readonly LocalizableString _menuTitle = new LocalizableString(nameof(MenuTitle));
        private readonly LocalizableString _confirmationQuestion = new LocalizableString(nameof(ConfirmationQuestion));
        private readonly LocalizableString _successMessage = new LocalizableString(nameof(SuccessMessage));
        private readonly LocalizableString _helpUri = new LocalizableString(nameof(HelpUri));

        #endregion

        #region All Constructors

        /// <summary>
        /// Default constructor.  All associated string properties and methods will return <c>null</c>.
        /// </summary>
        public UseCaseDescriptionAttribute() { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the ButtonTitle attribute property, which may be a resource key string.
        /// <para>
        /// Consumers must use the <see cref="GetButtonTitle"/> method to retrieve the UI display string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure a localized
        /// name for display.
        /// <para>
        /// The <see cref="GetButtonTitle"/> method will return either the literal, non-localized
        /// string or the localized string when <see cref="ResourceType"/> has been specified.
        /// </para>
        /// </remarks>
        /// <value>
        /// ButtonTitle is generally used as a title or tooltip for an UI button that invokes the use case.
        /// A <c>null</c> or empty string is legal, and consumers must allow for that.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
        public string ButtonTitle
        {
            get
            {
                return this._buttonTitle.Value;
            }
            set
            {
                if (this._buttonTitle.Value != value)
                {
                    this._buttonTitle.Value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the MenuTitle attribute property, which may be a resource key string.
        /// <para>
        /// Consumers must use the <see cref="GetMenuTitle"/> method to retrieve the UI display string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure a localized
        /// name for display.
        /// <para>
        /// The <see cref="GetMenuTitle"/> method will return either the literal, non-localized
        /// string or the localized string when <see cref="ResourceType"/> has been specified.
        /// </para>
        /// </remarks>
        /// <value>
        /// MenuTitle is generally used as a title for an UI (context or main) menu that invokes the use case.
        /// A <c>null</c> or empty string is legal, and consumers must allow for that.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
        public string MenuTitle
        {
            get
            {
                return this._menuTitle.Value;
            }
            set
            {
                if (this._menuTitle.Value != value)
                {
                    this._menuTitle.Value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the ConfirmationQuestion attribute property, which may be a resource key string.
        /// <para>
        /// Consumers must use the <see cref="GetConfirmationQuestion"/> method to retrieve the UI display string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure a localized
        /// description for display.
        /// <para>
        /// The <see cref="GetConfirmationQuestion"/> method will return either the literal, non-localized
        /// string or the localized string when <see cref="ResourceType"/> has been specified.
        /// </para>
        /// </remarks>
        /// <value>
        /// ConfirmationQuestion is generally used as a text for an UI conform dialog form.
        /// A <c>null</c> or empty string is legal, and consumers must allow for that.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
        public string ConfirmationQuestion
        {
            get
            {
                return this._confirmationQuestion.Value;
            }
            set
            {
                if (this._confirmationQuestion.Value != value)
                {
                    this._confirmationQuestion.Value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the SuccessMessage attribute property, which may be a resource key string.
        /// <para>
        /// Consumers must use the <see cref="GetSuccessMessage"/> method to retrieve the UI display string.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The property contains either the literal, non-localized string or the resource key
        /// to be used in conjunction with <see cref="ResourceType"/> to configure a localized
        /// description for display.
        /// <para>
        /// The <see cref="GetSuccessMessage"/> method will return either the literal, non-localized
        /// string or the localized string when <see cref="ResourceType"/> has been specified.
        /// </para>
        /// </remarks>
        /// <value>
        /// SuccessMessage is generally used as a text for a success notification in UI.
        /// A <c>null</c> or empty string is legal, and consumers must allow for that.
        /// </value>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The property and method are a matched pair")]
        public string SuccessMessage
        {
            get
            {
                return this._successMessage.Value;
            }
            set
            {
                if (this._successMessage.Value != value)
                {
                    this._successMessage.Value = value;
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
        /// <see cref="ButtonTitle"/>,, <see cref="ConfirmationQuestion"/> and <see cref="SuccessMessage"/>.
        /// Using <see cref="ResourceType"/> along with these Key properties, allows
        /// the <see cref="GetButtonTitle"/>, <see cref="GetConfirmationQuestion"/>
        /// and <see cref="GetSuccessMessage"/> methods to return localized values.
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

                    this._buttonTitle.ResourceType = value;
                    this._menuTitle.ResourceType = value;
                    this._confirmationQuestion.ResourceType = value;
                    this._successMessage.ResourceType = value;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the UI display string for ButtonTitle.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="ButtonTitle"/>
        /// or the localized string found when <see cref="ResourceType"/> has been specified
        /// and <see cref="ButtonTitle"/> represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="ButtonTitle"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="ButtonTitle"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// <para>
        /// Can return <c>null</c> and will not fall back onto other values, as it's more likely for the
        /// consumer to want to fall back onto the property name.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="ButtonTitle"/>
        /// property, but a public static property with a name matching the <see cref="ButtonTitle"/>
        /// value couldn't be found  on the <see cref="ResourceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
        public string GetButtonTitle()
        {
            return this._buttonTitle.GetLocalizableValue();
        }

        /// <summary>
        /// Gets the UI display string for MenuTitle.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="MenuTitle"/>
        /// or the localized string found when <see cref="ResourceType"/> has been specified
        /// and <see cref="MenuTitle"/> represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="MenuTitle"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="MenuTitle"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// <para>
        /// Can return <c>null</c> and will not fall back onto other values, as it's more likely for the
        /// consumer to want to fall back onto the property name.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="MenuTitle"/>
        /// property, but a public static property with a name matching the <see cref="MenuTitle"/>
        /// value couldn't be found  on the <see cref="ResourceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
        public string GetMenuTitle()
        {
            return this._menuTitle.GetLocalizableValue();
        }

        /// <summary>
        /// Gets the UI display string for ConfirmationQuestion.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="ConfirmationQuestion"/>
        /// or the localized string found when <see cref="ResourceType"/> has been specified
        /// and <see cref="ConfirmationQuestion"/> represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="ConfirmationQuestion"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="NameForOld"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="ConfirmationQuestion"/>
        /// property, but a public static property with a name matching the <see cref="ConfirmationQuestion"/>
        /// value couldn't be found on the <see cref="ResourceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
        public string GetConfirmationQuestion()
        {
            return this._confirmationQuestion.GetLocalizableValue();
        }

        /// <summary>
        /// Gets the UI display string for SuccessMessage.
        /// <para>
        /// This can be either a literal, non-localized string provided to <see cref="SuccessMessage"/>
        /// or the localized string found when <see cref="ResourceType"/> has been specified
        /// and <see cref="SuccessMessage"/> represents a resource key within that resource type.
        /// </para>
        /// </summary>
        /// <returns>
        /// When <see cref="ResourceType"/> has not been specified, the value of
        /// <see cref="SuccessMessage"/> will be returned.
        /// <para>
        /// When <see cref="ResourceType"/> has been specified and <see cref="SuccessMessage"/>
        /// represents a resource key within that resource type, then the localized value will be returned.
        /// </para>
        /// <para>
        /// Can return <c>null</c> and will not fall back onto other values, as it's more likely for the
        /// consumer to want to fall back onto the property name.
        /// </para>
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// After setting both the <see cref="ResourceType"/> property and the <see cref="SuccessMessage"/>
        /// property, but a public static property with a name matching the <see cref="SuccessMessage"/>
        /// value couldn't be found on the <see cref="ResourceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method does work using a property of the same name")]
        public string GetSuccessMessage()
        {
            return this._successMessage.GetLocalizableValue();
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
