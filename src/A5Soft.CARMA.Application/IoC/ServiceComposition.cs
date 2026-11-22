using A5Soft.CARMA.Application.IoC.Tenants;
using A5Soft.CARMA.Domain.UnitTests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Application.IoC
{
    /// <summary>
    /// The app's services (framework defaults included) and the plugins' overrides: an app scoped service or a
    /// singleton is overridden by at most one plugin, app wide; a tenant scoped service by any number of plugins,
    /// recorded by plugin identifier, of which a tenant may be assigned only one.
    /// </summary>
    internal sealed class ServiceComposition
    {
        private readonly List<ApplicationServiceInfo> _app;
        private readonly Dictionary<(Type, bool), (ApplicationServiceInfo Service, PluginAssembly Plugin)> _overrides
            = new Dictionary<(Type, bool), (ApplicationServiceInfo, PluginAssembly)>();
        private readonly Dictionary<Type, Dictionary<string, ApplicationServiceInfo>> _tenantOverrides
            = new Dictionary<Type, Dictionary<string, ApplicationServiceInfo>>();
        private readonly List<ApplicationServiceInfo> _added = new List<ApplicationServiceInfo>();
        private readonly List<ExclusivePlugins> _exclusive = new List<ExclusivePlugins>();


        /// <param name="app">the framework's and the app's services, composed</param>
        public ServiceComposition(List<ApplicationServiceInfo> app)
            => _app = app ?? throw new ArgumentNullException(nameof(app));


        /// <summary>Gets the plugin pairs overriding the same tenant scoped service.</summary>
        public IReadOnlyList<ExclusivePlugins> Exclusive => _exclusive;


        /// <summary>
        /// Adds a plugin's services; returns the conflicts. A refused plugin stops startup, so a partial add is
        /// never used.
        /// </summary>
        public List<ConventionViolation> Add(PluginAssembly plugin, IEnumerable<ApplicationServiceInfo> services)
        {
            var violations = new List<ConventionViolation>();

            foreach (var service in services)
            {
                if (service.AllowMultiple)
                {
                    // a set of implementations cannot be chosen per tenant
                    if (service.IsTenantScoped) violations.Add(new ConventionViolation(service.ImplementationType,
                        string.Empty, $"implements the tenant scoped {service.InterfaceType}, which allows multiple " +
                        $"implementations: a plugin cannot add one ({plugin.ToString()})."));
                    else _added.Add(service);
                    continue;
                }

                if (service.IsTenantScoped)
                {
                    if (!_tenantOverrides.TryGetValue(service.InterfaceType, out var byPlugin))
                        _tenantOverrides.Add(service.InterfaceType,
                            byPlugin = new Dictionary<string, ApplicationServiceInfo>(StringComparer.OrdinalIgnoreCase));

                    if (byPlugin.TryGetValue(plugin.Id, out var same))
                    {
                        violations.Add(Conflict(service, same.ImplementationType, plugin));
                        continue;
                    }

                    foreach (var other in byPlugin.Keys)
                        _exclusive.Add(new ExclusivePlugins(other, plugin.Id, service.InterfaceType));
                    byPlugin.Add(plugin.Id, service);
                    continue;
                }

                var key = Key(service);
                if (_overrides.TryGetValue(key, out var existing))
                {
                    violations.Add(Conflict(service, existing.Service.ImplementationType, existing.Plugin));
                    continue;
                }
                _overrides.Add(key, (service, plugin));
            }

            return violations;
        }

        /// <summary>
        /// Gets the services to register as they are, and the tenant scoped ones, chosen per scope.
        /// </summary>
        public (List<ApplicationServiceInfo> Services, List<TenantServiceInfo> TenantServices) Build()
        {
            var services = new List<ApplicationServiceInfo>();
            var defaults = new Dictionary<Type, ApplicationServiceInfo>();

            foreach (var service in _app.Where(s => s.AllowMultiple || !_overrides.ContainsKey(Key(s))))
            {
                if (service.IsTenantScoped && !service.AllowMultiple) defaults.Add(service.InterfaceType, service);
                else services.Add(service);
            }
            services.AddRange(_overrides.Values.Select(v => v.Service));
            services.AddRange(_added);

            var tenantServices = defaults.Keys.Union(_tenantOverrides.Keys).Select(type =>
            {
                defaults.TryGetValue(type, out var defaultService);
                _tenantOverrides.TryGetValue(type, out var byPlugin);
                var overrides = byPlugin
                    ?? new Dictionary<string, ApplicationServiceInfo>(StringComparer.OrdinalIgnoreCase);

                return new TenantServiceInfo(type, (defaultService ?? overrides.Values.First()).Lifetime,
                    defaultService, overrides);
            }).ToList();

            return (services, tenantServices);
        }


        // the same key as IoCManagerBase.Compose: by interface, or by implementation for a service without one
        private static (Type, bool) Key(ApplicationServiceInfo s)
            => (s.InterfaceType ?? s.ImplementationType, null == s.InterfaceType);

        private static ConventionViolation Conflict(ApplicationServiceInfo service, Type existing, PluginAssembly owner)
            => new ConventionViolation(service.ImplementationType, string.Empty,
                $"implements {service.InterfaceType ?? service.ImplementationType}, which {existing} " +
                $"(plugin {owner.ToString()}) already implements");
    }
}