using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Events;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Background.CronJobs
{
    /// <summary>
    /// A job run on a schedule (a cron expression in <see cref="ICronJobsConfiguration"/>), on a server only.
    /// </summary>
    /// <remarks>
    /// <para>An implementation is marked with <see cref="BackgroundKeyAttribute"/> (the key the configuration names
    /// it by) and <c>[DefaultServiceImplementation(typeof(ICronJob), BuildConfiguration.Server)]</c>: a cron job
    /// meant for any other build stops startup.</para>
    /// <para>In a tenant based app (<see cref="Tenancy.ITenantDirectory"/>) a job runs once per active tenant, each
    /// run in its own scope with the system identity for that tenant, failing and retrying on its own. A job marked
    /// <see cref="GlobalCronJobAttribute"/> runs once, without a tenant. A plugin's job runs only for the tenants
    /// assigned the plugin.</para>
    /// <para>Scoped: the scope's identity (the system identity, <see cref="Authorization.Sessions.IIdentityProvider.CreateSystem"/>)
    /// is set before the job is created, so it may depend on scoped and tenant scoped services,
    /// <see cref="IAppEventPublisher"/> included. Events it publishes have no publishing user, so their handlers that
    /// run as the publishing user (<see cref="RunAsPublishingUserAttribute"/>) are skipped.</para>
    /// <para>A run does not overlap with another run of the same job unless the job is marked
    /// <see cref="AllowConcurrentRunsAttribute"/> (enforced by the scheduler).</para>
    /// </remarks>
    [Service(ServiceLifetime.Scoped, allowMultipleImplementations: true)]
    public interface ICronJob : IBackgroundWork
    {
        /// <summary>
        /// Runs the job.
        /// </summary>
        /// <param name="cancellationToken">cancelled when the host shuts down: the run is then repeated later</param>
        Task RunAsync(CancellationToken cancellationToken);
    }
}
