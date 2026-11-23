using System;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations
{
    /// <summary>
    /// The default <see cref="IRuleProvider"/>: resolves the rules from the current scope, so that a tenant scoped
    /// rule gets the implementation for the scope's tenant, and a rule overridden by a plugin gets the override.
    /// </summary>
    [DefaultServiceImplementation(typeof(IRuleProvider))]
    public sealed class ServiceProviderRuleProvider : IRuleProvider
    {
        private readonly IServiceProvider _services;


        /// <summary>
        /// Creates a new rule provider.
        /// </summary>
        /// <param name="services">the service provider of the current scope</param>
        public ServiceProviderRuleProvider(IServiceProvider services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }


        /// <inheritdoc />
        public IValidationRule ResolveRule(Type ruleInterfaceType)
        {
            if (null == ruleInterfaceType) throw new ArgumentNullException(nameof(ruleInterfaceType));
            if (!typeof(IValidationRule).IsAssignableFrom(ruleInterfaceType)) throw new ArgumentException(
                $"{ruleInterfaceType.FullName} is not IValidationRule.", nameof(ruleInterfaceType));

            var result = _services.GetService(ruleInterfaceType);

            if (null == result) throw new InvalidOperationException(
                $"{ruleInterfaceType.FullName} is not configured in the IoC container.");
            
            return (IValidationRule)result;
        }
    }
}
