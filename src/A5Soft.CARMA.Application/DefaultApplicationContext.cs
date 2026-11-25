using A5Soft.CARMA.Application.IoC;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;
using System;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// The default domain context: what domain objects need, and nothing from the application layer, so that the
    /// client data portal can depend on it without depending on the application context, which holds the portal.
    /// </summary>
    /// <remarks>An application context should delegate to it (<c>ApplicationContextBase</c>), never replace it:
    /// a replacement depending on the client data portal would make a circular dependency.</remarks>
    [DefaultServiceImplementation(typeof(Domain.IDomainContext))]
    public sealed class DefaultApplicationContext : Domain.IDomainContext
    {
        /// <summary>
        /// Creates a new domain context.
        /// </summary>
        /// <param name="metadataProvider">the metadata provider</param>
        /// <param name="validationProvider">the validation engine provider of the scope</param>
        /// <param name="settings">the domain settings fixed at startup</param>
        public DefaultApplicationContext(IMetadataProvider metadataProvider,
            IValidationEngineProvider validationProvider, IDomainSettings settings)
        {
            MetadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
            ValidationProvider = validationProvider ?? throw new ArgumentNullException(nameof(validationProvider));
            BindingMode = (settings ?? throw new ArgumentNullException(nameof(settings))).BindingMode;
        }


        /// <inheritdoc />
        public IMetadataProvider MetadataProvider { get; }

        /// <inheritdoc />
        public IValidationEngineProvider ValidationProvider { get; }

        /// <inheritdoc />
        public BindingMode BindingMode { get; }
    }
}