using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// A background work provider in memory, for a thick client (the default there) and for tests: one worker runs
    /// the work items one at a time, each in a new scope (<see cref="IBackgroundScopeFactory"/>, which the app must
    /// register).
    /// </summary>
    /// <remarks>
    /// <para>Nothing survives the process: on dispose (the container disposed when the app closes) it stops taking
    /// new work, runs what is due within <see cref="DrainTimeout"/>, then cancels what is still running; every work
    /// item left is logged as critical with its string.</para>
    /// <para>A server can use it too (e.g. in development) through a derived class marked
    /// <c>[DefaultServiceImplementation(typeof(IBackgroundScheduler), BuildConfiguration.Server)]</c>; its cron
    /// schedules are then not run, as it has no cron engine.</para>
    /// </remarks>
    [DefaultServiceImplementation(typeof(IBackgroundScheduler), BuildConfiguration.Local)]
    public class InMemoryBackgroundScheduler : IBackgroundScheduler, IDisposable
    {
        private static readonly TimeSpan MaxWait = TimeSpan.FromMinutes(1);

        private readonly object _lock = new object();
        private readonly List<WorkItem> _queue = new List<WorkItem>();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
        private readonly CancellationTokenSource _stop = new CancellationTokenSource();
        private readonly ILogger _logger;
        private readonly IBackgroundScopeFactory _scopes;
        private Task _worker;
        private DateTime? _drainUntilUtc;
        private bool _closed;


        /// <summary>
        /// Creates the scheduler.
        /// </summary>
        /// <param name="logger">the logger</param>
        /// <param name="scopeFactory">creates the scope of each work item; null if the app registers none (then
        /// nothing can be enqueued)</param>
        public InMemoryBackgroundScheduler(ILogger logger, IBackgroundScopeFactory scopeFactory = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopes = scopeFactory;
        }


        /// <inheritdoc />
        public int MaxEnvelopeSize => 0;

        /// <summary>
        /// Gets how long dispose waits for the work due: 15 seconds by default.
        /// </summary>
        protected virtual TimeSpan DrainTimeout => TimeSpan.FromSeconds(15);


        /// <inheritdoc />
        public Task EnqueueAsync(EventEnvelope envelope, DateTime runAtUtc, CancellationToken cancellationToken)
        {
            if (null == envelope) throw new ArgumentNullException(nameof(envelope));
            Enqueue(new WorkItem(envelope.ToWire(), false, runAtUtc, envelope.ToString()));
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task EnqueueAsync(CronRun run, DateTime runAtUtc, CancellationToken cancellationToken)
        {
            if (null == run) throw new ArgumentNullException(nameof(run));
            Enqueue(new WorkItem(run.ToWire(), true, runAtUtc, run.ToString()));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops taking new work, runs what is due within <see cref="DrainTimeout"/> and logs what is left.
        /// </summary>
        public void Dispose()
        {
            Task worker;
            var timeout = DrainTimeout;
            lock (_lock)
            {
                if (null != _drainUntilUtc) return;
                _drainUntilUtc = DateTime.UtcNow.Add(timeout);
                worker = _worker;
                if (null == worker) _closed = true;
            }

            if (null == worker) return;

            _signal.Release();
            if (!worker.Wait(timeout))
            {
                // the work still running is cancelled; the worker then logs what is left
                _stop.Cancel();
                worker.Wait(TimeSpan.FromSeconds(5));
            }
        }


        private void Enqueue(WorkItem item)
        {
            if (null == _scopes) throw new InvalidOperationException(
                $"Register an {nameof(IBackgroundScopeFactory)}: the in-memory scheduler runs each work item in a new scope.");

            lock (_lock)
            {
                if (_closed) throw new ObjectDisposedException(GetType().Name,
                    "The background scheduler has stopped: the app is closing.");

                var index = _queue.FindIndex(q => q.DueUtc > item.DueUtc);
                if (index < 0) _queue.Add(item);
                else _queue.Insert(index, item);

                if (null == _worker) _worker = Task.Run(RunAsync);
            }

            _signal.Release();
        }

        private async Task RunAsync()
        {
            while (true)
            {
                WorkItem next = null;
                var wait = TimeSpan.Zero;
                var stop = false;

                lock (_lock)
                {
                    var now = DateTime.UtcNow;
                    var first = _queue.Count > 0 ? _queue[0] : null;

                    if (null != _drainUntilUtc && (now >= _drainUntilUtc || _stop.IsCancellationRequested))
                        stop = true;
                    else if (null != first && first.DueUtc <= now)
                    {
                        next = first;
                        _queue.RemoveAt(0);
                    }
                    else if (null != _drainUntilUtc && (null == first || first.DueUtc >= _drainUntilUtc))
                        stop = true;
                    else
                    {
                        var until = first?.DueUtc ?? now.Add(MaxWait);
                        if (null != _drainUntilUtc && until > _drainUntilUtc.Value) until = _drainUntilUtc.Value;
                        wait = until - now;
                        if (wait > MaxWait) wait = MaxWait;
                    }
                }

                if (stop) break;

                if (null == next)
                {
                    await _signal.WaitAsync(wait).ConfigureAwait(false);
                    continue;
                }

                await ExecuteAsync(next).ConfigureAwait(false);
            }

            WorkItem[] left;
            lock (_lock)
            {
                _closed = true;
                left = _queue.ToArray();
                _queue.Clear();
            }

            foreach (var item in left) _logger.LogCritical(
                $"Background work not run before the app closed: {item.Description}, due {item.DueUtc:O}, work item: {item.Wire}");
        }

        private async Task ExecuteAsync(WorkItem item)
        {
            try
            {
                using (var scope = _scopes.CreateScope())
                {
                    var runner = (IBackgroundWorkRunner)scope.Services.GetService(typeof(IBackgroundWorkRunner))
                        ?? throw new InvalidOperationException($"{nameof(IBackgroundWorkRunner)} is not registered.");

                    if (item.IsCron)
                    {
                        var run = CronRun.FromWire(item.Wire);
                        var result = await runner.RunCronAsync(run, _stop.Token).ConfigureAwait(false);
                        if (result.Status == WorkStatus.Retry) Requeue(run.NextAttempt().ToWire(), item, result);
                    }
                    else
                    {
                        var envelope = EventEnvelope.FromWire(item.Wire);
                        var result = await runner.RunEventAsync(envelope, _stop.Token).ConfigureAwait(false);
                        if (result.Status == WorkStatus.Retry) Requeue(envelope.NextAttempt().ToWire(), item, result);
                    }
                }
            }
            catch (Exception ex) when (ex.IsCancellation(_stop.Token))
            {
                _logger.LogCritical($"Background work interrupted as the app closed: {item.Description}, work item: {item.Wire}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"The background scheduler has failed to run {item.Description}, work item: {item.Wire}", ex);
            }
        }

        private void Requeue(string wire, WorkItem item, WorkResult result)
        {
            var retry = new WorkItem(wire, item.IsCron, result.RetryAtUtc ?? DateTime.UtcNow, item.Description);
            lock (_lock)
            {
                // still open while draining: the retry runs if it falls due in time, otherwise it is logged as left
                if (_closed) return;
                var index = _queue.FindIndex(q => q.DueUtc > retry.DueUtc);
                if (index < 0) _queue.Add(retry);
                else _queue.Insert(index, retry);
            }
        }


        private sealed class WorkItem
        {
            public WorkItem(string wire, bool isCron, DateTime dueUtc, string description)
            {
                Wire = wire;
                IsCron = isCron;
                DueUtc = dueUtc.Kind == DateTimeKind.Local ? dueUtc.ToUniversalTime()
                    : DateTime.SpecifyKind(dueUtc, DateTimeKind.Utc);
                Description = description;
            }

            public string Wire { get; }
            public bool IsCron { get; }
            public DateTime DueUtc { get; }
            public string Description { get; }
        }
    }
}
