using System;

namespace A5Soft.CARMA.Application.Background.CronJobs
{
    /// <summary>
    /// Marks a cron job (<see cref="ICronJob"/>) whose runs may overlap: a run may start while the previous one (or,
    /// in a tenant based app, the run for another tenant) is still running. Without it the scheduler runs them one at
    /// a time (<see cref="CronSchedule.AllowConcurrentRuns"/>).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class AllowConcurrentRunsAttribute : Attribute { }
}
