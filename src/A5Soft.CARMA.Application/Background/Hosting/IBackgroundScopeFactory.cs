using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// Creates the DI scope a work item runs in. Implemented by the app over its container, e.g. over
    /// <c>IServiceScopeFactory</c> of Microsoft.Extensions.DependencyInjection; the framework does not depend on a
    /// container.
    /// </summary>
    /// <remarks>Needed wherever work items run: by the scheduler's jobs on a server, by
    /// <see cref="InMemoryBackgroundScheduler"/> in a thick client.</remarks>
    [Service(ServiceLifetime.Singleton)]
    public interface IBackgroundScopeFactory
    {
        /// <summary>
        /// Creates a new scope, with no identity set yet.
        /// </summary>
        IBackgroundScope CreateScope();
    }
}
