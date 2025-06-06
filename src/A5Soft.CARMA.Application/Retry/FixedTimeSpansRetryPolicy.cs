using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Application.Retry
{
    /// <summary>
    /// A retry policy using the fixed list of time spans.
    /// </summary>
    [Serializable]
    public class FixedTimeSpansRetryPolicy : IRetryPolicy
    {
        /// <summary>
        /// Serialization constructor.
        /// </summary>
        /// <param name="delayTimeSpans">Time spans to delay for each retry (at least one).</param>
        public FixedTimeSpansRetryPolicy(List<TimeSpan> delayTimeSpans)
        {
            if (null == delayTimeSpans || delayTimeSpans.Count < 1) throw new ArgumentOutOfRangeException(nameof(delayTimeSpans),
                $"Delay time spans cannot be null or empty.");

            DelayTimeSpans = delayTimeSpans;
        }

        /// <summary>
        /// Creates a new policy instance for the parameters provided.
        /// </summary>
        /// <param name="delayTimeSpans">Time spans to delay for each retry (at least one).</param>
        public FixedTimeSpansRetryPolicy(params TimeSpan[] delayTimeSpans)
        {
            if (null == delayTimeSpans || delayTimeSpans.Length < 1) throw new ArgumentOutOfRangeException(nameof(delayTimeSpans),
                $"Delay time spans cannot be null or empty.");

            DelayTimeSpans = new List<TimeSpan>(delayTimeSpans);
        }


        /// <summary>
        /// How many retry attempts to take (min 1, max 20).
        /// </summary>
        public int RetryAttempts => DelayTimeSpans?.Count ?? 0;

        /// <summary>
        /// Time spans to delay for each retry.
        /// </summary>
        public List<TimeSpan> DelayTimeSpans { get; }


        /// <inheritdoc cref="IRetryPolicy.GetDelayForRetry"/>
        public TimeSpan GetDelayForRetry(int attempt)
        {
            if (attempt < 1) throw new ArgumentOutOfRangeException(nameof(attempt),
                $"Retry attempt value {attempt} is invalid (min 1).");

            var index = Math.Min(DelayTimeSpans.Count, attempt);

            return DelayTimeSpans[index-1];
        }
    }
}
