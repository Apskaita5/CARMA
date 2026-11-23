using System;

namespace A5Soft.CARMA.Application.DataPortal.EndPoints
{
    /// <summary>
    /// Where the client data portal sends its calls: a transport (<see cref="IDataPortalTransport"/>) and, for a
    /// remote one, the absolute URL of the server's data portal endpoint. Immutable.
    /// </summary>
    public sealed class DataPortalEndpoint : IEquatable<DataPortalEndpoint>
    {
        /// <summary>The name of the framework's local transport: use cases run in process, nothing is sent.</summary>
        public const string Local = "local";

        /// <summary>The name of the framework's HTTP transport.</summary>
        public const string Http = "http";


        /// <summary>
        /// Creates a new endpoint.
        /// </summary>
        /// <param name="transport">the transport name (compared ignoring case)</param>
        /// <param name="address">the absolute URL of the server's data portal endpoint; null for a transport
        /// without one (local)</param>
        /// <exception cref="ArgumentException">the address is not an absolute URL</exception>
        public DataPortalEndpoint(string transport, Uri address = null)
        {
            if (string.IsNullOrWhiteSpace(transport)) throw new ArgumentNullException(nameof(transport));
            if (null != address && !address.IsAbsoluteUri) throw new ArgumentException(
                "The server address must be an absolute URL.", nameof(address));

            Transport = transport.Trim();
            Address = address;
        }


        /// <summary>Gets the transport name (compared ignoring case).</summary>
        public string Transport { get; }

        /// <summary>Gets the absolute URL of the server's data portal endpoint; null for a transport without one.</summary>
        public Uri Address { get; }


        /// <inheritdoc />
        public bool Equals(DataPortalEndpoint other) => null != other
            && string.Equals(Transport, other.Transport, StringComparison.OrdinalIgnoreCase)
            && Equals(Address, other.Address);

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as DataPortalEndpoint);

        /// <inheritdoc />
        public override int GetHashCode()
            => StringComparer.OrdinalIgnoreCase.GetHashCode(Transport) ^ (Address?.GetHashCode() ?? 0);

        /// <inheritdoc />
        public override string ToString() => null == Address ? Transport : $"{Transport} {Address}";
    }
}
