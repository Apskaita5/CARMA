using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Authorization.Identity
{
    /// <summary>
    /// Runs code on the UI thread through its synchronization context: for WinForms
    /// (<c>WindowsFormsSynchronizationContext</c>) and WPF (<c>DispatcherSynchronizationContext</c>). Capture the context
    /// on the UI thread at startup (before <c>Application.Run</c>) and register the instance as the app's
    /// <see cref="IUiDispatcher"/>, e.g. with <c>SystemServiceCollection.AddInstance</c>.
    /// </summary>
    public sealed class SynchronizationContextUiDispatcher : IUiDispatcher
    {
        private readonly SynchronizationContext _context;


        /// <summary>Creates the dispatcher.</summary>
        /// <param name="context">the UI thread's synchronization context</param>
        public SynchronizationContextUiDispatcher(SynchronizationContext context)
            => _context = context ?? throw new ArgumentNullException(nameof(context));


        /// <inheritdoc />
        public Task InvokeAsync(Func<Task> action)
        {
            if (null == action) throw new ArgumentNullException(nameof(action));

            var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _context.Post(async _ =>
            {
                try
                {
                    await action();
                    completion.SetResult(true);
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            }, null);

            return completion.Task;
        }
    }
}
