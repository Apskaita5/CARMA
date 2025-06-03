using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.Retry
{
    /// <summary>
    /// A retry policy using the Fibonacci sequence to determine retry delays.
    /// Each retry delay is the sum of the two previous delays.
    /// It grows slower than exponential backoff but faster than linear,
    /// which can be useful when you want gentle retry escalation without going "too big too fast".
    /// </summary>
    [Serializable]
    public class FibonacciBackoffRetryPolicy
    {
        // Efficient Fibonacci with memoization
        private static Dictionary<int, long> _fibCache = new Dictionary<int, long> { { 1, 1 }, { 2, 1 } };


        /// <summary>
        /// Creates a new policy instance for the parameters provided.
        /// </summary>
        /// <param name="retryAttempts">How many retry attempts to take (min 1, max 20).</param>
        /// <param name="timeUnit">Time unit for Fibonacci steps (e.g., seconds, minutes).</param>
        /// <param name="maxDelay">Maximum delay allowed (e.g., 1 minute).</param>
        public FibonacciBackoffRetryPolicy(int retryAttempts, TimeSpan timeUnit, TimeSpan maxDelay)
        {
            if (retryAttempts < 1 || retryAttempts > 20) throw new ArgumentOutOfRangeException(nameof(retryAttempts),
                $"Retry attempts value {retryAttempts} is invalid (min 1, max 20).");

            RetryAttempts = retryAttempts;
            TimeUnit = timeUnit;
            MaxDelay = maxDelay;
        }

        /// <summary>
        /// Creates a new policy instance for the default parameters:
        /// <see cref="TimeUnit"/> - 1 sec.
        /// <see cref="MaxDelay"/> - 5 min.
        /// </summary>
        /// <param name="retryAttempts">How many retry attempts to take (min 1, max 20).</param>
        public FibonacciBackoffRetryPolicy(int retryAttempts)
            : this(retryAttempts, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(5)) {}

        /// <summary>
        /// Creates a new policy instance for the default parameters:
        /// <see cref="RetryAttempts"/> - 3
        /// <see cref="TimeUnit"/> - 1 sec.
        /// <see cref="MaxDelay"/> - 5 min.
        /// </summary>
        public FibonacciBackoffRetryPolicy()
            : this(3, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(5)) { }



        /// <summary>
        /// How many retry attempts to take (min 1, max 20).
        /// </summary>
        public int RetryAttempts { get; }

        /// <summary>
        /// Time unit for Fibonacci steps (e.g., seconds, minutes).
        /// </summary>
        public TimeSpan TimeUnit { get; }

        /// <summary>
        /// Maximum delay allowed (e.g., 1 minute).
        /// </summary>
        public TimeSpan MaxDelay { get; }


        /// <inheritdoc cref="IRetryPolicy.GetDelayForRetry"/>
        public TimeSpan GetDelayForRetry(int attempt)
        {
            if (attempt < 1 || attempt > 20) throw new ArgumentOutOfRangeException(nameof(attempt),
                $"Retry attempt value {attempt} is invalid (min 1, max 20).");

            // Generate Fibonacci number for this attempt
            long fib = Fibonacci(attempt);
            var delay = TimeSpan.FromMilliseconds(TimeUnit.TotalMilliseconds * fib);

            return delay > MaxDelay ? MaxDelay : delay;
        }


        private static long Fibonacci(int n)
        {
            if (_fibCache.ContainsKey(n)) return _fibCache[n];

            long value = Fibonacci(n - 1) + Fibonacci(n - 2);
            _fibCache[n] = value;

            return value;
        }
    }
}
