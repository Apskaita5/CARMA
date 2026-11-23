using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base class for app configuration POCO's.
    /// </summary>
    public abstract class ConfigurationBase
    {
        /// <summary>
        /// A type of the configuration interface to add as a singleton service.
        /// </summary>
        public abstract Type InterfaceType { get; }

        /// <summary>
        /// Gets the configuration section the class is bound from: by default the class name (e.g. a
        /// <c>PluginsConfiguration</c> section). Override only to bind from another section.
        /// </summary>
        public virtual string RootSection => GetType().Name;

        /// <summary>
        /// The app build configurations (client, server or both) this configuration is for. A configuration
        /// for another build is not bound. Default: both.
        /// </summary>
        /// <remarks>Read from a new instance, so return a constant.</remarks>
        public virtual BuildConfiguration ForBuildConfiguration => BuildConfiguration.Any;

        /// <summary>
        /// Override the method to init configuration with calculated values.
        /// </summary>
        /// <param name="appPath">a path where the app files are located</param>
        /// <param name="isDevelopment">whether its a development environment</param>
        public virtual void Init(string appPath, bool isDevelopment)
        { }

        /// <summary>
        /// Validates the state of the config data. Throws exception if not valid.
        /// </summary>
        public virtual void Validate()
        { }
    }
}
