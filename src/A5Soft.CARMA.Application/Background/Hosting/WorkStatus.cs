namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// What became of a work item (<see cref="WorkResult"/>).
    /// </summary>
    public enum WorkStatus
    {
        /// <summary>Done, or skipped as not applicable: drop the work item.</summary>
        Succeeded = 0,

        /// <summary>Failed transiently: enqueue its next attempt at <see cref="WorkResult.RetryAtUtc"/>.</summary>
        Retry = 1,

        /// <summary>Abandoned, and logged as critical: drop it (or keep it as failed, as the scheduler does).</summary>
        Failed = 2
    }
}
