using System;

namespace A5Soft.CARMA.Application.Retry
{
    /// <summary>
    /// A retry policy using exponential backoff (retry intervals grow exponentially)
    /// and jitter (randomized delays help avoid thundering herd problems).
    /// </summary>
    [Serializable]
    public class ExponentialBackoffWithJitterRetryPolicy : IRetryPolicy
    {
        private static readonly Random _random = new Random();


        /// <summary>
        /// Creates a new policy instance.
        /// </summary>
        /// <param name="retryAttempts">How many retry attempts to take (min 1, max 20).</param>
        /// <param name="baseDelay">Base delay (e.g., 2 seconds).</param>
        /// <param name="maxDelay">Maximum delay allowed (e.g., 1 minute).</param>
        /// <param name="backoffFactor">Multiplier per attempt (base of the exponent).</param>
        public ExponentialBackoffWithJitterRetryPolicy(int retryAttempts, TimeSpan baseDelay, TimeSpan maxDelay, double backoffFactor)
        {
            if (retryAttempts < 1 || retryAttempts > 20) throw new ArgumentOutOfRangeException(nameof(retryAttempts),
                $"Retry attempts value {retryAttempts} is invalid (min 1, max 20).");

            RetryAttempts = retryAttempts;
            BaseDelay = baseDelay;
            MaxDelay = maxDelay;
            BackoffFactor = backoffFactor;
        }

        /// <summary>
        /// Creates a new policy instance for default parameters:
        /// <see cref="BaseDelay"/> - 2 sec.
        /// <see cref="MaxDelay"/> - 1 min.
        /// <see cref="BackoffFactor"/> - 2.0.
        /// </summary>
        /// <param name="retryAttempts">How many retry attempts to take (min 1, max 20).</param>
        public ExponentialBackoffWithJitterRetryPolicy(int retryAttempts)
            : this(retryAttempts, TimeSpan.FromSeconds(2), TimeSpan.FromMinutes(1), 2.0) {}

        /// <summary>
        /// Creates a new policy instance for default parameters:
        /// <see cref="RetryAttempts"/> - 3
        /// <see cref="BaseDelay"/> - 2 sec.
        /// <see cref="MaxDelay"/> - 1 min.
        /// <see cref="BackoffFactor"/> - 2.0.
        /// </summary>
        public ExponentialBackoffWithJitterRetryPolicy()
            : this(3, TimeSpan.FromSeconds(2), TimeSpan.FromMinutes(1), 2.0) { }


        /// <summary>
        /// How many retry attempts to take (min 1, max 20).
        /// </summary>
        public int RetryAttempts { get; }

        /// <summary>
        /// Base delay (e.g., 2 seconds).
        /// </summary>
        public TimeSpan BaseDelay { get; }

        /// <summary>
        /// Maximum delay allowed (e.g., 1 minute).
        /// </summary>
        public TimeSpan MaxDelay { get; }

        /// <summary>
        /// Multiplier per attempt (base of the exponent).
        /// </summary>
        public double BackoffFactor { get; }


        /// <inheritdoc cref="IRetryPolicy.GetDelayForRetry"/>
        public TimeSpan GetDelayForRetry(int attempt)
        {
            if (attempt < 1 || attempt > 20) throw new ArgumentOutOfRangeException(nameof(attempt),
                $"Retry attempt value {attempt} is invalid (min 1, max 20).");

            // Exponential backoff: base * (factor ^ (n - 1))
            double exponentialDelay = BaseDelay.TotalMilliseconds * Math.Pow(BackoffFactor, attempt - 1);

            // Add jitter: random between 0.5x and 1.5x of calculated delay
            double jitterFactor = 0.5 + _random.NextDouble(); // [0.5, 1.5)
            double jitteredDelay = exponentialDelay * jitterFactor;

            // Clamp to max delay
            double finalDelay = Math.Min(jitteredDelay, MaxDelay.TotalMilliseconds);

            return TimeSpan.FromMilliseconds(finalDelay);
        }
    }
}
