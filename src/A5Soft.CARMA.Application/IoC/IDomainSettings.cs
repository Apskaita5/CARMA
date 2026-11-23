using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.IoC
{
    /// <summary>
    /// Domain settings fixed at startup by the IoC manager, from the kind of app (not from configuration).
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IDomainSettings
    {
        /// <summary>
        /// Gets the binding mode of the app's UI framework, for every domain object the app creates.
        /// </summary>
        BindingMode BindingMode { get; }
    }

    /// <summary>
    /// The domain settings, added to the container as an instance by the IoC manager.
    /// </summary>
    public sealed class DomainSettings : IDomainSettings
    {
        /// <summary>
        /// Creates new domain settings.
        /// </summary>
        /// <param name="bindingMode">the binding mode of the app's UI framework</param>
        public DomainSettings(BindingMode bindingMode) => BindingMode = bindingMode;


        /// <inheritdoc />
        public BindingMode BindingMode { get; }
    }
}
