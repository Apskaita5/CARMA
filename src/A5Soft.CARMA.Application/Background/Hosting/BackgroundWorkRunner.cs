using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Sessions;
using A5Soft.CARMA.Application.Background.CronJobs;
using A5Soft.CARMA.Application.Background.Internal;
using A5Soft.CARMA.Application.IoC.Plugins;
using A5Soft.CARMA.Application.Retry;
using A5Soft.CARMA.Application.Tenancy;
using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// The framework's <see cref="IBackgroundWorkRunner"/>.
    /// </summary>
    /// <remarks>
    /// <para>The identity, as the handler declares it (<see cref="RunAsPublishingUserAttribute"/>) when it runs: the
    /// system identity (<see cref="IIdentityProvider.CreateSystem"/>, which must be an administrator), or the
    /// publishing user's as <see cref="IIdentityProvider.FetchAsync"/> fetches it now (upgraded to an administrator if
    /// the handler says so); a cron job always runs with the system identity. For a tenant, with the tenant's plugins
    /// (<see cref="ITenantPluginProvider"/>, checked by <see cref="TenantServiceMap.ValidateAssignment"/>) appended
    /// as <see cref="CarmaIdentity.WithSession"/> does for a session. It has no ticket, so nothing checks it against
    /// the identity store.</para>
    /// <para>A user who is disabled or deleted fails the work item without a retry, and so does a handler that runs as
    /// the publishing user for a work item without one (the handler was changed after the event was published);
    /// a plugin's handler or job for a
    /// tenant no longer assigned the plugin is skipped; a handler, job or event type no longer registered (a plugin
    /// removed, a key changed) fails the work item.</para>
    /// <para>Logging: a failure not logged yet (by a use case the work called) is logged as a warning if transient,
    /// otherwise as an error, with the work item's correlation id; business exceptions included, as nobody sees them
    /// in the background. A work item abandoned is logged once more as critical, without the stack trace.</para>
    /// </remarks>
    [DefaultServiceImplementation(typeof(IBackgroundWorkRunner), BuildConfiguration.Backend)]
    public class BackgroundWorkRunner : IBackgroundWorkRunner
    {
        private readonly IServiceProvider _services;
        private readonly BackgroundWorkRegistry _registry;
        private readonly IAuthenticationStateProvider _authentication;
        private readonly IIdentityProvider _identities;
        private readonly TenantServiceMap _tenantServices;
        private readonly IBackgroundScheduler _scheduler;
        private readonly ILogger _logger;
        private readonly ITenantPluginProvider _tenantPlugins;
        private readonly ITenantDirectory _tenants;


        /// <summary>
        /// Creates the runner of a scope.
        /// </summary>
        /// <param name="services">the scope's provider: the handler or job is resolved from it</param>
        /// <param name="registry">the event types, handlers and cron jobs</param>
        /// <param name="authentication">the scope's identity holder</param>
        /// <param name="tenantServices">the tenant service map, to check a tenant's plugins</param>
        /// <param name="logger">the logger</param>
        /// <param name="identities">the app's identity provider; null in a build without one (then a work item that
        /// runs fails, logged as critical)</param>
        /// <param name="scheduler">the background work provider, to enqueue the runs for the tenants; null in a build
        /// without one (then nothing runs work items)</param>
        /// <param name="tenantPlugins">the tenants' plugins; null if the app does not assign plugins to tenants</param>
        /// <param name="tenants">the active tenants; null if the app is not tenant based</param>
        /// <remarks>Everything a build may lack is optional, so that a build without background work (no provider, no
        /// identity provider) still passes a container's validation of every registration (e.g. the
        /// <c>ValidateOnBuild</c> of the generic host in development).</remarks>
        public BackgroundWorkRunner(IServiceProvider services, BackgroundWorkRegistry registry,
            IAuthenticationStateProvider authentication, TenantServiceMap tenantServices, ILogger logger,
            IIdentityProvider identities = null, IBackgroundScheduler scheduler = null,
            ITenantPluginProvider tenantPlugins = null, ITenantDirectory tenants = null)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
            _tenantServices = tenantServices ?? throw new ArgumentNullException(nameof(tenantServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _identities = identities;
            _scheduler = scheduler;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantPlugins = tenantPlugins;
            _tenants = tenants;
        }


        /// <inheritdoc />
        public async Task<WorkResult> RunEventAsync(EventEnvelope envelope, CancellationToken cancellationToken)
        {
            if (null == envelope) throw new ArgumentNullException(nameof(envelope));

            var description = envelope.ToString();

            if (!_registry.TryGetHandler(envelope.HandlerKey, out var handler))
                return Abandon(description, envelope.CorrelationId, "the handler is not registered (any more)");
            if (!_registry.TryGetEventType(envelope.EventKey, out var eventType))
                return Abandon(description, envelope.CorrelationId, "the event type is not registered (any more)");
            if (!handler.EventType.IsAssignableFrom(eventType))
                return Abandon(description, envelope.CorrelationId,
                    $"the handler does not handle {eventType.FullName} (any more)");

            IBackgroundWork work = null;
            try
            {
                string userId = null;
                if (handler.RunsAsPublishingUser)
                {
                    if (string.IsNullOrWhiteSpace(envelope.UserId)) return Abandon(description, envelope.CorrelationId,
                        "the handler runs as the publishing user, and the work item has none (the handler was changed " +
                        "after the event was published)");
                    userId = envelope.UserId;
                }

                var identity = await CreateIdentityAsync(userId, handler.AsAdmin, envelope.TenantId, cancellationToken)
                    .ConfigureAwait(false);
                if (null == identity) return Abandon(description, envelope.CorrelationId,
                    $"user {envelope.UserId} may no longer log in (disabled, deleted or removed from the tenant)");

                if (!IsAssigned(handler.PluginId, identity, description)) return WorkResult.Succeeded;

                await _authentication.InitializeAsync(identity, true, cancellationToken).ConfigureAwait(false);

                var appEvent = AppEventSerializer.Deserialize(envelope.Payload, eventType);
                work = (IBackgroundWork)Resolve(handler.HandlerType);

                await handler.InvokeAsync(work, appEvent, cancellationToken).ConfigureAwait(false);
                return WorkResult.Succeeded;
            }
            catch (Exception ex) when (!ex.IsCancellation(cancellationToken))
            {
                return Fail(ex, work, envelope.Attempt, envelope.CorrelationId, description);
            }
        }

        /// <inheritdoc />
        public async Task<WorkResult> RunCronAsync(CronRun run, CancellationToken cancellationToken)
        {
            if (null == run) throw new ArgumentNullException(nameof(run));

            run = run.WithCorrelationId(WireFormat.NewId());
            var description = run.ToString();

            if (!_registry.TryGetCronJob(run.JobKey, out var job))
                return Abandon(description, run.CorrelationId, "the cron job is not registered (any more)");

            if (null == run.TenantId && !job.IsGlobal && null != _tenants)
                return await FanOutAsync(run, description, cancellationToken).ConfigureAwait(false);

            IBackgroundWork work = null;
            try
            {
                var identity = await CreateIdentityAsync(null, false, run.TenantId, cancellationToken)
                    .ConfigureAwait(false);

                if (!IsAssigned(job.PluginId, identity, description)) return WorkResult.Succeeded;

                await _authentication.InitializeAsync(identity, true, cancellationToken).ConfigureAwait(false);

                work = (IBackgroundWork)Resolve(job.JobType);

                await ((ICronJob)work).RunAsync(cancellationToken).ConfigureAwait(false);
                return WorkResult.Succeeded;
            }
            catch (Exception ex) when (!ex.IsCancellation(cancellationToken))
            {
                return Fail(ex, work, run.Attempt, run.CorrelationId, description);
            }
        }


        /// <summary>
        /// Enqueues the run of the firing for each active tenant. Not retried as a whole, which would run the other
        /// tenants twice: a tenant whose run cannot be enqueued misses this firing (logged as critical).
        /// </summary>
        private async Task<WorkResult> FanOutAsync(CronRun run, string description, CancellationToken cancellationToken)
        {
            if (null == _scheduler) return Abandon(description, run.CorrelationId,
                $"no {nameof(IBackgroundScheduler)} is registered to run it for each tenant");

            IReadOnlyCollection<string> tenants;
            try
            {
                tenants = await _tenants.GetActiveTenantsAsync(cancellationToken).ConfigureAwait(false)
                    ?? Array.Empty<string>();
            }
            catch (Exception ex) when (!ex.IsCancellation(cancellationToken))
            {
                // nothing enqueued yet: the whole firing can be retried
                return Fail(ex, null, run.Attempt, run.CorrelationId, description);
            }

            foreach (var tenantId in tenants.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var tenantRun = run.ForTenant(tenantId, run.CorrelationId);
                try
                {
                    await _scheduler.EnqueueAsync(tenantRun, DateTime.UtcNow, CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical($"The background work provider has failed to take {tenantRun}, correlation id " +
                        $"{run.CorrelationId}: the tenant misses this run, work item: {tenantRun.ToWire()}", ex);
                }
            }

            return WorkResult.Succeeded;
        }

        /// <summary>
        /// Creates the identity to run with, for the tenant given (if any).
        /// </summary>
        /// <param name="userId">the user to run as; null for the system identity</param>
        /// <param name="asAdmin">whether to upgrade the user's identity to an administrator</param>
        /// <param name="tenantId">the tenant; null for none</param>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        /// <returns>the identity; null if the user may no longer log in</returns>
        private async Task<CarmaIdentity> CreateIdentityAsync(string userId, bool asAdmin, string tenantId,
            CancellationToken cancellationToken)
        {
            if (null == _identities) throw new InvalidOperationException(
                $"Background work runs with an identity from the app's {nameof(IIdentityProvider)}, and this build " +
                "has none: register the app's implementation for it.");

            CarmaIdentity identity;
            if (null == userId)
            {
                identity = _identities.CreateSystem() ?? throw new InvalidOperationException(
                    $"{nameof(IIdentityProvider)}.{nameof(IIdentityProvider.CreateSystem)} has returned no identity.");
                if (!identity.IsAdmin) throw new InvalidOperationException(
                    $"{nameof(IIdentityProvider)}.{nameof(IIdentityProvider.CreateSystem)} has returned an identity " +
                    "that is not an administrator: the system identity holds every permission.");
                identity = identity.AsSystem();
            }
            else
            {
                identity = await _identities.FetchAsync(userId, tenantId, cancellationToken).ConfigureAwait(false);
                if (null == identity) return null;
                if (identity.UserId != userId) throw new InvalidOperationException(
                    $"{nameof(IIdentityProvider)} has fetched another user than {userId}.");
                if (asAdmin) identity = identity.AsAdmin();
            }

            if (string.IsNullOrWhiteSpace(tenantId)) return identity;

            var assigned = null == _tenantPlugins
                ? Array.Empty<string>()
                : await _tenantPlugins.GetPluginsAsync(tenantId, cancellationToken).ConfigureAwait(false);
            var plugins = _tenantServices.ValidateAssignment(tenantId, assigned);

            return identity.WithSession(null, tenantId, plugins, false);
        }

        /// <summary>
        /// Gets whether a plugin's work applies to the identity's tenant: always for the app's own work and without a
        /// tenant (a plugin is then installed app wide).
        /// </summary>
        private bool IsAssigned(string pluginId, CarmaIdentity identity, string description)
        {
            if (null == pluginId || null == identity.TenantId
                || identity.TenantPlugins.Contains(pluginId, StringComparer.OrdinalIgnoreCase)) return true;

            _logger.LogDebug($"Skipped {description}: tenant {identity.TenantId} is not assigned plugin {pluginId}.");
            return false;
        }

        private object Resolve(Type type)
            => _services.GetService(type) ?? throw new InvalidOperationException(
                $"{type.FullName} is not registered in the container: the IoC manager registers every event " +
                "handler and cron job under its own type.");

        private WorkResult Fail(Exception exception, IBackgroundWork work, int attempt, string correlationId,
            string description)
        {
            correlationId = LogFailure(exception, description, correlationId);

            if (null != exception.ToTransientException())
            {
                // before the work is created (fetching the identity, the tenants): the default policy
                DateTime? next = null;
                try
                {
                    next = null == work
                        ? BackgroundRetry.GetRetryTime(BackgroundRetry.DefaultPolicy, attempt + 1)
                        : work.GetRetryTime(attempt + 1, exception);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{work?.GetType().FullName}.{nameof(IBackgroundWork.GetRetryTime)} has failed.", ex);
                }

                if (next.HasValue)
                {
                    var atUtc = next.Value.Kind == DateTimeKind.Local ? next.Value.ToUniversalTime()
                        : DateTime.SpecifyKind(next.Value, DateTimeKind.Utc);
                    _logger.LogInfo($"{description} will be attempted again at {atUtc:O}, correlation id {correlationId}.");
                    return WorkResult.Retry(atUtc);
                }
            }

            return Abandon(description, correlationId,
                $"{exception.GetType().Name}: {exception.Message}");
        }

        /// <summary>
        /// Logs a failure, unless a use case called by the work has logged it already.
        /// </summary>
        /// <returns>the correlation id it is logged under</returns>
        private string LogFailure(Exception exception, string description, string correlationId)
        {
            var id = exception.GetCorrelationId() ?? correlationId;
            if (exception.IsLogged()) return id;

            if (null != exception.ToTransientException()) _logger.LogWarning(exception, description, id);
            else _logger.LogError($"{description} has failed, correlation id {id}.", exception);

            exception.MarkLogged(id);
            return id;
        }

        private WorkResult Abandon(string description, string correlationId, string reason)
        {
            _logger.LogCritical($"Background work abandoned: {description}: {reason}. Correlation id {correlationId}.");
            return WorkResult.Failed;
        }
    }
}
