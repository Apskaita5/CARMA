using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace A5Soft.CARMA.Application.Authorization.Sessions
{
    /// <summary>
    /// Issues and validates tickets signed with HMAC-SHA256 by random keys held in memory, rotated at an interval and
    /// kept as long as a ticket they signed can be valid. Format: <c>v1.{keyId}.{base64url payload}.{base64url signature}</c>
    /// (printable ASCII without spaces, as the data portal requires).
    /// </summary>
    public sealed class TicketSigner
    {
        private const string Version = "v1";
        private const int MaxTicketLength = 4096;

        private readonly object _lock = new object();
        private readonly List<(int Id, byte[] Key, DateTimeOffset Created)> _keys = new List<(int, byte[], DateTimeOffset)>();
        private readonly TimeSpan _rotation;
        private readonly TimeSpan _retention;
        private int _lastKeyId;


        public TicketSigner(TimeSpan rotation, TimeSpan maxTicketLifetime)
        {
            _rotation = rotation;
            // a key signs for one rotation interval; a ticket it signed can live a max lifetime longer
            _retention = rotation + maxTicketLifetime;
        }


        public string Issue(TicketInfo ticket, DateTimeOffset now)
        {
            var payload = new JObject
            {
                ["i"] = ticket.TicketId,
                ["u"] = ticket.UserId,
                ["t"] = ticket.TenantId,
                ["s"] = ticket.IdentityStamp,
                ["ia"] = ticket.IssuedAt.ToUnixTimeSeconds(),
                ["e"] = ticket.ExpiresAt.ToUnixTimeSeconds()
            };

            var (id, key) = GetCurrentKey(now);
            var signed = $"{Version}.{id}.{Base64Url(Encoding.UTF8.GetBytes(payload.ToString(Newtonsoft.Json.Formatting.None)))}";

            return $"{signed}.{Base64Url(Sign(key, signed))}";
        }

        public TicketInfo Validate(string ticket, DateTimeOffset now)
        {
            if (string.IsNullOrEmpty(ticket) || ticket.Length > MaxTicketLength) return null;

            var parts = ticket.Split('.');
            if (parts.Length != 4 || parts[0] != Version || !int.TryParse(parts[1], out var keyId)) return null;

            byte[] key;
            lock (_lock) key = _keys.FirstOrDefault(k => k.Id == keyId).Key;
            if (null == key) return null;

            var signature = FromBase64Url(parts[3]);
            var expected = Sign(key, $"{parts[0]}.{parts[1]}.{parts[2]}");
            if (null == signature || !CryptographicOperations.FixedTimeEquals(signature, expected)) return null;

            try
            {
                var payload = JObject.Parse(Encoding.UTF8.GetString(FromBase64Url(parts[2]) ?? Array.Empty<byte>()));
                var result = new TicketInfo((string)payload["i"], (string)payload["u"], (string)payload["t"],
                    (string)payload["s"], DateTimeOffset.FromUnixTimeSeconds((long)payload["ia"]),
                    DateTimeOffset.FromUnixTimeSeconds((long)payload["e"]));

                return result.ExpiresAt > now ? result : null;
            }
            catch (Exception)
            {
                // signed by this server, so never expected; refused all the same
                return null;
            }
        }


        private (int Id, byte[] Key) GetCurrentKey(DateTimeOffset now)
        {
            lock (_lock)
            {
                _keys.RemoveAll(k => now - k.Created > _retention);

                var current = _keys.LastOrDefault();
                if (null == current.Key || now - current.Created >= _rotation)
                {
                    var key = new byte[32];
                    using (var random = RandomNumberGenerator.Create()) random.GetBytes(key);
                    current = (++_lastKeyId, key, now);
                    _keys.Add(current);
                }

                return (current.Id, current.Key);
            }
        }

        private static byte[] Sign(byte[] key, string data)
        {
            using (var hmac = new HMACSHA256(key))
                return hmac.ComputeHash(Encoding.ASCII.GetBytes(data));
        }

        internal static string Base64Url(byte[] data)
            => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static byte[] FromBase64Url(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var base64 = value.Replace('-', '+').Replace('_', '/');
            base64 += new string('=', (4 - base64.Length % 4) % 4);
            try
            {
                return Convert.FromBase64String(base64);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
