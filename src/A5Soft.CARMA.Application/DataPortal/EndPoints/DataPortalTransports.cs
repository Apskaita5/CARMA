using A5Soft.CARMA.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Application.DataPortal.EndPoints
{
    /// <summary>
    /// The default <see cref="IDataPortalTransports"/>.
    /// </summary>
    [DefaultServiceImplementation(typeof(IDataPortalTransports))]
    public sealed class DataPortalTransports : IDataPortalTransports
    {
        private readonly Dictionary<string, IDataPortalTransport> _transports;
        private readonly ConcurrentDictionary<DataPortalEndpoint, IDataPortalProxy> _proxies
            = new ConcurrentDictionary<DataPortalEndpoint, IDataPortalProxy>();


        /// <summary>
        /// Creates the transport registry.
        /// </summary>
        /// <param name="transports">every transport registered</param>
        /// <exception cref="InvalidOperationException">two transports have the same name</exception>
        public DataPortalTransports(IEnumerable<IDataPortalTransport> transports)
        {
            var list = (transports ?? throw new ArgumentNullException(nameof(transports)))
                .Where(t => null != t).ToList();

            var duplicate = list.GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
            if (null != duplicate) throw new InvalidOperationException(
                $"Data portal transports {string.Join(" and ", duplicate.Select(t => t.GetType().FullName))} " +
                $"have the same name {duplicate.Key}.");

            _transports = list.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);
        }


        /// <inheritdoc />
        public IReadOnlyCollection<string> Names => _transports.Keys;


        /// <inheritdoc />
        public IDataPortalProxy GetProxy(DataPortalEndpoint endpoint)
        {
            if (null == endpoint) throw new ArgumentNullException(nameof(endpoint));

            if (!_transports.TryGetValue(endpoint.Transport, out var transport)) throw new ArgumentException(
                $"Unknown data portal transport {endpoint.Transport}; available: {string.Join(", ", Names)}.",
                nameof(endpoint));
            if (transport.RequiresAddress != (null != endpoint.Address)) throw new ArgumentException(
                transport.RequiresAddress
                    ? $"Data portal transport {transport.Name} needs a server address."
                    : $"Data portal transport {transport.Name} takes no server address.", nameof(endpoint));

            return _proxies.GetOrAdd(endpoint, e => transport.CreateProxy(e) ?? throw new InvalidOperationException(
                $"Data portal transport {transport.Name} created no proxy for {e}."));
        }
    }
}
