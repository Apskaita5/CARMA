using System;

namespace A5Soft.CARMA.Application.Retry
{
    /// <summary>
    /// Policy settings for retry behaviour.
    /// </summary>
    public interface IRetryPolicy
    {
        /// <summary>
        /// How many retry attempts to take.
        /// </summary>
        int RetryAttempts { get; }

        /// <summary>
        /// Gets a delay for the retry attempt.
        /// </summary>
        /// <param name="attempt">no of the attempt to get a dalay for</param>
        /// <returns>a delay for the retry attempt</returns>
        TimeSpan GetDelayForRetry(int attempt);
    }
}
