using A5Soft.CARMA.Application.DataPortal.EndPoints;
using System;

namespace A5Soft.CARMA.Application.DataPortal.Client
{
    /// <summary>
    /// The configured data portal endpoint, from the <c>DataPortalConnectionConfiguration</c> section.
    /// </summary>
    public sealed class DataPortalConnectionConfiguration : ConfigurationBase, IDataPortalConnectionConfiguration
    {
        /// <inheritdoc />
        public override Type InterfaceType => typeof(IDataPortalConnectionConfiguration);

        /// <summary>Gets or sets the transport name, e.g. "http" or "local"; empty if chosen at runtime.</summary>
        public string Transport { get; set; }

        /// <summary>Gets or sets the absolute URL of the server's data portal endpoint, for a remote transport.</summary>
        public string Address { get; set; }

        /// <inheritdoc />
        public DataPortalEndpoint Endpoint { get; private set; }


        /// <inheritdoc />
        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(Transport))
            {
                if (!string.IsNullOrWhiteSpace(Address)) throw new InvalidOperationException(
                    $"{nameof(DataPortalConnectionConfiguration)}:{nameof(Address)} is set without a {nameof(Transport)}.");
                return;
            }

            Uri address = null;
            if (!string.IsNullOrWhiteSpace(Address) && !Uri.TryCreate(Address, UriKind.Absolute, out address))
                throw new InvalidOperationException(
                    $"{nameof(DataPortalConnectionConfiguration)}:{nameof(Address)} must be an absolute URL.");

            // the transport name and the address requirement are checked when the endpoint is first used
            Endpoint = new DataPortalEndpoint(Transport, address);
        }
    }
}
