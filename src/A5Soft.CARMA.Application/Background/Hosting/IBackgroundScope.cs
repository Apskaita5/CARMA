using System;

namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// A DI scope a work item runs in (<see cref="IBackgroundScopeFactory"/>): dispose it when the work item is done.
    /// </summary>
    public interface IBackgroundScope : IDisposable
    {
        /// <summary>
        /// Gets the provider of the scope.
        /// </summary>
        IServiceProvider Services { get; }
    }
}
