using System;
using System.Threading.Tasks;
using A5Soft.CARMA.Domain;

namespace A5Soft.CARMA.Application.Authorization.Identity
{
    /// <summary>
    /// Runs code on the UI thread: for UI frameworks with thread affinity (WinForms, WPF), so that identity change
    /// handlers can update controls. Optional: registered only by such apps (e.g. a
    /// <see cref="Default.SynchronizationContextUiDispatcher"/> added in <see cref="IoC.IoCManagerBase"/>'s
    /// AddSystemServices); Blazor components marshal themselves with InvokeAsync.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface IUiDispatcher
    {
        /// <summary>Runs an action on the UI thread.</summary>
        /// <returns>a task completed when the action has completed</returns>
        Task InvokeAsync(Func<Task> action);
    }
}
