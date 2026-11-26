using A5Soft.CARMA.Application.Background.Events;
using A5Soft.CARMA.Domain.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Background.Hosting
{
    /// <summary>
    /// An event handler found by the startup scan (<see cref="BackgroundWorkRegistry"/>).
    /// </summary>
    public sealed class AppEventHandlerInfo
    {
        private readonly Func<object, IAppEvent, CancellationToken, Task> _invoke;


        internal AppEventHandlerInfo(string key, Type handlerType, Type eventType, string pluginId)
        {
            Key = key;
            HandlerType = handlerType;
            EventType = eventType;
            PluginId = pluginId;

            var runAs = (RunAsPublishingUserAttribute)Attribute.GetCustomAttribute(handlerType,
                typeof(RunAsPublishingUserAttribute), true);
            RunsAsPublishingUser = null != runAs;
            AsAdmin = runAs?.AsAdmin ?? false;

            var method = typeof(HandlerInvoker<>).MakeGenericType(eventType)
                .GetMethod(nameof(HandlerInvoker<IAppEvent>.InvokeAsync));
            _invoke = (Func<object, IAppEvent, CancellationToken, Task>)Delegate.CreateDelegate(
                typeof(Func<object, IAppEvent, CancellationToken, Task>), method);
        }


        /// <summary>Gets the handler's key (<see cref="BackgroundKeyAttribute"/>).</summary>
        public string Key { get; }

        /// <summary>Gets the handler's type: registered under itself too, so that the runner resolves exactly it.</summary>
        public Type HandlerType { get; }

        /// <summary>Gets the event type the handler declares: it handles that type and every type deriving from it.</summary>
        public Type EventType { get; }

        /// <summary>Gets the identifier of the plugin the handler comes from; null for the app's own.</summary>
        public string PluginId { get; }

        /// <summary>
        /// Gets whether the handler runs as the user who published the event (<see cref="RunAsPublishingUserAttribute"/>);
        /// otherwise as the system identity.
        /// </summary>
        public bool RunsAsPublishingUser { get; }

        /// <summary>Gets whether the publishing user's identity is upgraded to an administrator.</summary>
        public bool AsAdmin { get; }


        /// <summary>Invokes the handler.</summary>
        internal Task InvokeAsync(object handler, IAppEvent appEvent, CancellationToken cancellationToken)
            => _invoke(handler, appEvent, cancellationToken);


        private static class HandlerInvoker<TEvent> where TEvent : IAppEvent
        {
            // public: found by name through reflection
            public static Task InvokeAsync(object handler, IAppEvent appEvent, CancellationToken cancellationToken)
                => ((IAppEventHandler<TEvent>)handler).HandleAsync((TEvent)appEvent, cancellationToken);
        }
    }
}
