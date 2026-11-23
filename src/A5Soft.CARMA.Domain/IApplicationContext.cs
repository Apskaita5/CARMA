using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// A container interface for app wide settings and services.
    /// </summary>
    [Service(ServiceLifetime.Scoped)]
    public interface IApplicationContext
    {
        /// <summary>
        /// Metadata provider.
        /// </summary>
        public IMetadataProvider MetadataProvider { get; }

        /// <summary>
        /// Validation engine provider.
        /// </summary>
        public IValidationEngineProvider ValidationProvider { get; }

        /// <summary>
        /// Gets a binding mode (for the current GUI framework in use by the app installation).
        /// E.g. for WinForms binding schema there should be only one PropertyChanged event
        /// raised per range of changed properties, while for WPF binding schema an event per
        /// each property in range should be raised.
        /// </summary>
        BindingMode BindingMode { get; }
    }
}
