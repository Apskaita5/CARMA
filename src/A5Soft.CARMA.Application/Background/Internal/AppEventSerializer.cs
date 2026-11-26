using A5Soft.CARMA.Application.DataPortal.Protocol;
using A5Soft.CARMA.Domain.Events;
using Newtonsoft.Json;
using System;

namespace A5Soft.CARMA.Application.Background.Internal
{
    /// <summary>
    /// Serializes an event by the data portal's rules (<see cref="DataPortalSerialization"/>): what can cross the data
    /// portal can be stored as an event, and nothing else (no type values, no file content).
    /// </summary>
    internal static class AppEventSerializer
    {
        /// <summary>The max nesting depth of an event's JSON.</summary>
        public const int MaxDepth = 64;


        public static string Serialize(IAppEvent appEvent)
        {
            try
            {
                return JsonConvert.SerializeObject(appEvent, appEvent.GetType(),
                    DataPortalSerialization.GetSerializerSettings(MaxDepth));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Event {appEvent.GetType().FullName} cannot be serialized: {ex.Message}", ex);
            }
        }

        public static IAppEvent Deserialize(string payload, Type eventType)
            => (IAppEvent)JsonConvert.DeserializeObject(payload, eventType,
                    DataPortalSerialization.GetSerializerSettings(MaxDepth))
                ?? throw new FormatException($"The payload holds no {eventType.FullName}.");
    }
}
