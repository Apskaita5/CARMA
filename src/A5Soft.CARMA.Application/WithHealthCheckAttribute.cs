using System;
using System.Linq;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Attribute denoting that the application service has a health check.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class WithHealthCheckAttribute : Attribute
    {
        private Type _healthCheckType;

        /// <summary>
        /// Attribute denoting that the application service has a health check.
        /// </summary>
        /// <param name="healthCheckType">a type of the health check service, should be a class service implementing <see cref="IHealthCheck"/></param>
        public WithHealthCheckAttribute(Type healthCheckType)
        {
            _healthCheckType = healthCheckType ?? throw new ArgumentNullException(nameof(healthCheckType));
        }

        /// <summary>
        /// a type of the health check service, should be a class service implementing <see cref="IHealthCheck"/>
        /// </summary>
        public Type HealthCheckType
        {
            get => _healthCheckType;
            set
            {
                if (null == value) throw new ArgumentNullException(nameof(value));
                if (!value.IsClass || value.IsAbstract || !value.GetInterfaces().Any(i => i == typeof(IHealthCheck)))
                    throw new InvalidOperationException($"Type {value.FullName} is not IHealthCheck.");
                _healthCheckType = value;
            }
        }
    }
}
