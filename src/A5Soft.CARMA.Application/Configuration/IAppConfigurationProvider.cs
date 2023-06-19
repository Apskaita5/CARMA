using System;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// App configuration provider that creates a poco configuration object,
    /// binds it to the actual app configuration data and adds as a singleton service.
    /// </summary>
    public interface IAppConfigurationProvider
    {
        /// <summary>
        /// Creates a poco configuration object of the specified type,
        /// binds it to the actual app configuration data and adds as a singleton service.
        /// </summary>
        /// <param name="configurationType">a type of the poco configuration object to add</param>
        void AddConfigurationFor(Type configurationType);
    }
}
