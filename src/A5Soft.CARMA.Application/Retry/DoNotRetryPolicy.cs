using System;

namespace A5Soft.CARMA.Application.Retry
{
    /// <summary>
    /// A retry policy requiring NOT to do retry.
    /// </summary>
    [Serializable]
    public class DoNotRetryPolicy : IRetryPolicy
    {
        /// <inheritdoc cref="IRetryPolicy.RetryAttempts"/>
        public int RetryAttempts => 0;

        /// <inheritdoc cref="IRetryPolicy.GetDelayForRetry"/>
        public TimeSpan GetDelayForRetry(int attempt) => TimeSpan.FromMilliseconds(10);
    }
}
