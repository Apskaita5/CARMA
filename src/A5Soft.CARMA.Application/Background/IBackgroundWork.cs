using System;

namespace A5Soft.CARMA.Application.Background
{
    /// <summary>
    /// What every piece of background work (an event handler, a cron job) tells the runner about retries.
    /// </summary>
    public interface IBackgroundWork
    {
        /// <summary>
        /// Gets the time of the next attempt after a transient failure. The framework decides whether to retry (only
        /// a transient failure is retried, see <see cref="Retry.TransientException"/>); the work decides when.
        /// </summary>
        /// <param name="failedAttempts">the number of attempts that have failed so far, this one included (1 after
        /// the first failure)</param>
        /// <param name="error">the failure</param>
        /// <returns>the time of the next attempt (UTC); null to give up</returns>
        DateTime? GetRetryTime(int failedAttempts, Exception error);
    }
}
