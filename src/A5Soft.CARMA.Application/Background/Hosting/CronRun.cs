using A5Soft.CARMA.Application.Background.Internal;
using Newtonsoft.Json;
using System;

namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// A stored work item: one run of a cron job. Without a tenant it is the firing of the schedule (which, for a job
    /// running per tenant, enqueues one run per active tenant); with one, the run for that tenant.
    /// </summary>
    /// <remarks>A scheduler stores it as one opaque string (<see cref="ToWire"/>, read back by
    /// <see cref="FromWire"/>). Immutable.</remarks>
    public sealed class CronRun
    {
        private string _wire;


        [JsonConstructor]
        private CronRun() { }

        /// <summary>
        /// Creates the firing of a cron job's schedule: what a scheduler fires on each occurrence of the cron
        /// expression.
        /// </summary>
        /// <param name="jobKey">the key of the cron job (<see cref="CronSchedule.Key"/>)</param>
        public CronRun(string jobKey)
        {
            if (string.IsNullOrWhiteSpace(jobKey)) throw new ArgumentNullException(nameof(jobKey));
            JobKey = jobKey;
        }


        /// <summary>Gets the key of the cron job.</summary>
        [JsonProperty("k")]
        public string JobKey { get; private set; }

        /// <summary>Gets the tenant of the run; null for the firing of the schedule (or a global job).</summary>
        [JsonProperty("t")]
        public string TenantId { get; private set; }

        /// <summary>Gets the number of attempts made before this one: 0 for the first.</summary>
        [JsonProperty("n")]
        public int Attempt { get; private set; }

        /// <summary>
        /// Gets the correlation id: shared by the runs of one firing, and in the log entries of their failures;
        /// null in a schedule's firing, which gets one when it runs.
        /// </summary>
        [JsonProperty("c")]
        public string CorrelationId { get; private set; }


        /// <summary>
        /// Gets the work item as one string, to store.
        /// </summary>
        public string ToWire() => _wire ?? (_wire = WireFormat.Write(this));

        /// <summary>
        /// Reads a work item stored by <see cref="ToWire"/>.
        /// </summary>
        /// <exception cref="FormatException">the string is not a cron job work item</exception>
        public static CronRun FromWire(string wire)
        {
            var result = WireFormat.Read<CronRun>(wire);
            if (string.IsNullOrWhiteSpace(result.JobKey)) throw new FormatException("A work item is not a cron job run.");
            return result;
        }

        /// <summary>
        /// Gets the same run for its next attempt.
        /// </summary>
        public CronRun NextAttempt()
        {
            var result = Copy();
            result.Attempt = Attempt + 1;
            return result;
        }

        /// <summary>Gets the run of the same firing for a tenant, first attempt.</summary>
        internal CronRun ForTenant(string tenantId, string correlationId)
        {
            var result = Copy();
            result.TenantId = tenantId;
            result.Attempt = 0;
            result.CorrelationId = correlationId;
            return result;
        }

        /// <summary>Gets the run with a correlation id, if it has none.</summary>
        internal CronRun WithCorrelationId(string correlationId)
        {
            if (null != CorrelationId) return this;
            var result = Copy();
            result.CorrelationId = correlationId;
            return result;
        }

        private CronRun Copy()
        {
            var result = (CronRun)MemberwiseClone();
            result._wire = null;
            return result;
        }

        /// <inheritdoc />
        public override string ToString()
            => $"cron job {JobKey}{(null == TenantId ? string.Empty : $" for tenant {TenantId}")}, attempt {Attempt + 1}";
    }
}
