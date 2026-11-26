using A5Soft.CARMA.Application.Retry;
using A5Soft.CARMA.Domain.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Background.Events
{
    /// <summary>
    /// A base for event handlers: retries a transient failure by <see cref="RetryPolicy"/>.
    /// </summary>
    /// <typeparam name="TEvent">the type of the events handled</typeparam>
    public abstract class AppEventHandlerBase<TEvent> : IAppEventHandler<TEvent> where TEvent : IAppEvent
    {
        /// <inheritdoc />
        public abstract Task HandleAsync(TEvent appEvent, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the retry policy: by default <see cref="BackgroundRetry.DefaultPolicy"/>.
        /// </summary>
        protected virtual IRetryPolicy RetryPolicy => BackgroundRetry.DefaultPolicy;

        /// <inheritdoc />
        public virtual DateTime? GetRetryTime(int failedAttempts, Exception error)
            => BackgroundRetry.GetRetryTime(RetryPolicy, failedAttempts);
    }
}
