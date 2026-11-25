using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.Authorization.Sessions;
using A5Soft.CARMA.Application.DataPortal.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace A5Soft.CARMA.Application.DataPortal.Internal
{
    /// <summary>
    /// The identity update a response carries: <c>{"ticket": "..."}</c> for a new ticket of the session (sliding
    /// expiration), or <c>{"ticket": "...", "identityType": "...", "identity": {...}}</c> for a refreshed identity.
    /// </summary>
    internal static class IdentityUpdate
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);


        /// <summary>
        /// Writes an update: the identity if its stamp differs from the one the caller's ticket was issued with, the
        /// ticket alone if only the ticket has changed; nothing otherwise.
        /// </summary>
        public static ReadOnlyMemory<byte> Create(string callerTicket, TicketInfo callerTicketInfo, CarmaIdentity current)
        {
            if (null == current || null == current.Ticket) return default;

            var identityChanged = !string.Equals(callerTicketInfo?.IdentityStamp, current.Stamp, StringComparison.Ordinal);
            if (!identityChanged && string.Equals(callerTicket, current.Ticket, StringComparison.Ordinal)) return default;

            var serializer = DataPortalSerialization.CreateSerializer(DataPortalSerialization.PortalMessageMaxDepth);
            var update = new JObject { ["ticket"] = current.Ticket };
            if (identityChanged)
            {
                update["identityType"] = RemoteUseCaseDescriptor.GetName(current.GetType());
                update["identity"] = JToken.FromObject(current, serializer);
            }

            return Utf8.GetBytes(update.ToString(Formatting.None));
        }

        /// <summary>
        /// Reads an update and applies it to the caller's identity.
        /// </summary>
        /// <returns>the updated identity; null if the update is empty</returns>
        /// <exception cref="JsonException">the update is malformed, or the identity is of another type</exception>
        public static CarmaIdentity Apply(ReadOnlyMemory<byte> update, CarmaIdentity current, Type identityType)
        {
            if (update.IsEmpty) return null;

            var serializer = DataPortalSerialization.CreateSerializer(DataPortalSerialization.PortalMessageMaxDepth);
            JObject json;
            using (var textReader = new StreamReader(update.OpenRead(), Utf8))
            using (var jsonReader = new JsonTextReader(textReader) { MaxDepth = DataPortalSerialization.PortalMessageMaxDepth })
            {
                json = JObject.Load(jsonReader);
            }

            var ticket = (string)json["ticket"];
            if (string.IsNullOrEmpty(ticket)) throw new JsonSerializationException("The identity update has no ticket.");

            var identity = json["identity"];
            if (null == identity || identity.Type == JTokenType.Null) return current.WithTicket(ticket);

            if (null == identityType || (string)json["identityType"] != RemoteUseCaseDescriptor.GetName(identityType))
                throw new JsonSerializationException("The identity update holds another identity type than the client's.");

            var result = (CarmaIdentity)identity.ToObject(identityType, serializer)
                ?? throw new JsonSerializationException("The identity update holds no identity.");
            if (result.Ticket != ticket) throw new JsonSerializationException("The identity update is inconsistent.");

            return result;
        }
    }
}
