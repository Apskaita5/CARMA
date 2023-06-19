using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace A5Soft.CARMA.Application.Configuration
{
    public static class Extensions
    {
        public static void AddAppConfiguration(this IAppConfigurationProvider configurationProvider,
            params Assembly[] forAssemblies)
        {
            if (null == configurationProvider) throw new ArgumentNullException(nameof(configurationProvider));
            if (null == forAssemblies || forAssemblies.Length < 1) throw new ArgumentNullException(nameof(forAssemblies));

            var configTypes = new List<Type>();
            foreach (var assembly in forAssemblies)
            {
                configTypes.AddRange(assembly.GetTypes().Where(t => t.IsClass
                    && t.BaseType == typeof(ConfigurationBase)));
            }

            foreach (var configType in configTypes)
            {
                configurationProvider.AddConfigurationFor(configType);
            }
        }
    }
}
