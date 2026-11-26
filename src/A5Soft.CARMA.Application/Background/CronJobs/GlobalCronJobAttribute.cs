using System;

namespace A5Soft.CARMA.Application.Background.CronJobs
{
    /// <summary>
    /// Marks a cron job (<see cref="ICronJob"/>) that runs once per schedule, without a tenant, also in a tenant based
    /// app: e.g. housekeeping of the administrative database. Without it a job runs once per active tenant. Has no
    /// effect in an app that is not tenant based.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class GlobalCronJobAttribute : Attribute { }
}
