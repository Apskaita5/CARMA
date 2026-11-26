using A5Soft.CARMA.Application.Retry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Background.CronJobs
{
    /// <summary>
    /// A base for cron jobs: retries a transient failure by <see cref="RetryPolicy"/>.
    /// </summary>
    public abstract class CronJobBase : ICronJob
    {
        /// <inheritdoc />
        public abstract Task RunAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the retry policy: by default <see cref="BackgroundRetry.DefaultPolicy"/>. A job run often may prefer
        /// not to retry at all (<see cref="DoNotRetryPolicy"/>) and wait for its next run.
        /// </summary>
        protected virtual IRetryPolicy RetryPolicy => BackgroundRetry.DefaultPolicy;

        /// <inheritdoc />
        public virtual DateTime? GetRetryTime(int failedAttempts, Exception error)
            => BackgroundRetry.GetRetryTime(RetryPolicy, failedAttempts);
    }
}
