using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Application.Tenancy
{
    /// <summary>
    /// The default <see cref="ITenantContext"/>: set once per scope.
    /// </summary>
    [DefaultServiceImplementation(typeof(ITenantContext))]
    public sealed class TenantContext : ITenantContext
    {
        private string _tenantId;
        private IReadOnlyCollection<string> _plugins;


        /// <inheritdoc />
        public bool IsInitialized { get; private set; }

        /// <inheritdoc />
        public string TenantId => IsInitialized ? _tenantId : throw NotInitialized();

        /// <inheritdoc />
        public IReadOnlyCollection<string> Plugins => IsInitialized ? _plugins : throw NotInitialized();


        /// <inheritdoc />
        public void Initialize(string tenantId, IEnumerable<string> plugins)
        {
            if (IsInitialized) throw new InvalidOperationException("The tenant of a scope cannot change.");

            _tenantId = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId;
            _plugins = null == _tenantId || null == plugins
                ? (IReadOnlyCollection<string>)Array.Empty<string>()
                : plugins.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            IsInitialized = true;
        }

        private static InvalidOperationException NotInitialized() => new InvalidOperationException(
            "The tenant of this scope is not set yet: nothing tenant scoped can be resolved before it is.");
    }
}
