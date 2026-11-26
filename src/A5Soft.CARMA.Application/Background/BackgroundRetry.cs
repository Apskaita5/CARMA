using A5Soft.CARMA.Application.Retry;
using System;

namespace A5Soft.CARMA.Application.Background
{
    /// <summary>
    /// Retry times for background work from an <see cref="IRetryPolicy"/>.
    /// </summary>
    public static class BackgroundRetry
    {
        /// <summary>
        /// Gets the default policy of background work: 5 retries, Fibonacci steps of 30 seconds (30 s, 30 s, 1 min,
        /// 1.5 min, 2.5 min), at most 30 minutes apart.
        /// </summary>
        public static IRetryPolicy DefaultPolicy { get; }
            = new FibonacciBackoffRetryPolicy(5, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(30));


        /// <summary>
        /// Gets the time of the next attempt by a policy.
        /// </summary>
        /// <param name="policy">the policy</param>
        /// <param name="failedAttempts">the number of attempts that have failed so far (1 after the first failure)</param>
        /// <returns>now plus the policy's delay for the retry; null once the policy's retries are used up</returns>
        public static DateTime? GetRetryTime(IRetryPolicy policy, int failedAttempts)
        {
            if (null == policy) throw new ArgumentNullException(nameof(policy));
            if (failedAttempts < 1 || failedAttempts > policy.RetryAttempts) return null;

            return DateTime.UtcNow.Add(policy.GetDelayForRetry(failedAttempts));
        }
    }
}
