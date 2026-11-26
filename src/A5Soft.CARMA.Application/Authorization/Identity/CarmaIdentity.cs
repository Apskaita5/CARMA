using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using A5Soft.CARMA.Application.DataPortal.EndPoints;
using A5Soft.CARMA.Application.DataPortal.Protocol;
using Newtonsoft.Json;

namespace A5Soft.CARMA.Application.Authorization.Identity
{
    /// <summary>
    /// The identity of a user and of the session it belongs to: the user's data as the app stores it (fetched by the
    /// app's <see cref="IIdentityProvider"/>), and the session state the framework adds on top (ticket, tenant,
    /// tenant plugins, endpoint).
    /// </summary>
    /// <remarks>
    /// <para>Every app defines exactly one concrete identity type deriving from this class, in an app assembly (not
    /// in a plugin): the startup scan finds it, and the data portal reads and writes identities as that type.</para>
    /// <para>An identity is immutable: the server caches one instance and shares it across concurrent calls. The
    /// derived type has only read-only fields and properties, a private constructor marked with
    /// <see cref="Domain.SerializationConstructorAttribute"/> whose parameters match its properties (the user data
    /// of this class included), and implements <see cref="Clone"/> through <see cref="CarmaIdentity(CarmaIdentity)"/>.
    /// The framework changes the session state only on a fresh clone, before anyone else sees it.</para>
    /// <para>Before login a scope holds an anonymous identity (<see cref="CreateAnonymous"/>): never null.</para>
    /// <para>Never put secrets in an identity: it travels to the client whole.</para>
    /// </remarks>
    public abstract class CarmaIdentity
    {
        private readonly HashSet<Guid> _permissions;


        /// <summary>
        /// Creates the user data of an identity, as the app's <see cref="IIdentityProvider"/> fetches it.
        /// </summary>
        /// <param name="userId">the user's identifier</param>
        /// <param name="userName">the user's name (login)</param>
        /// <param name="email">the user's email; null if none</param>
        /// <param name="phone">the user's phone; null if none</param>
        /// <param name="stamp">a value that changes whenever anything above (or the app's own data) changes, e.g. a
        /// security stamp column changed on every security edit</param>
        /// <param name="permissions">the identifiers of the permissions granted (<see cref="Permission.Id"/>)</param>
        /// <param name="isAdmin">whether the user has every permission, at every level</param>
        /// <param name="isTenantAdmin">whether the user has every tenant scoped permission within the tenant the
        /// identity is fetched for</param>
        protected CarmaIdentity(string userId, string userName, string email, string phone, string stamp,
            IReadOnlyCollection<Guid> permissions, bool isAdmin, bool isTenantAdmin)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrWhiteSpace(stamp)) throw new ArgumentNullException(nameof(stamp));

            UserId = userId;
            UserName = userName;
            Email = email;
            Phone = phone;
            Stamp = stamp;
            IsAdmin = isAdmin;
            IsTenantAdmin = isTenantAdmin;
            Permissions = (permissions ?? Array.Empty<Guid>()).Distinct().ToArray();
            _permissions = new HashSet<Guid>(Permissions);
            TenantPlugins = Array.Empty<string>();
        }

        /// <summary>
        /// Copies an identity, its session state included: for <see cref="Clone"/> in a derived class.
        /// </summary>
        /// <param name="source">the identity to copy</param>
        protected CarmaIdentity(CarmaIdentity source)
        {
            if (null == source) throw new ArgumentNullException(nameof(source));

            UserId = source.UserId;
            UserName = source.UserName;
            Email = source.Email;
            Phone = source.Phone;
            Stamp = source.Stamp;
            IsAdmin = source.IsAdmin;
            IsTenantAdmin = source.IsTenantAdmin;
            Permissions = source.Permissions;
            _permissions = source._permissions;
            Ticket = source.Ticket;
            TenantId = source.TenantId;
            TenantPlugins = source.TenantPlugins;
            IsSecondFactorPending = source.IsSecondFactorPending;
            Endpoint = source.Endpoint;
            IsSystem = source.IsSystem;
            IsElevated = source.IsElevated;
        }

        /// <summary>For the anonymous identity only.</summary>
        internal CarmaIdentity()
        {
            Permissions = Array.Empty<Guid>();
            _permissions = new HashSet<Guid>();
            TenantPlugins = Array.Empty<string>();
        }


        #region User data

        /// <summary>Gets the user's identifier; null for an anonymous identity.</summary>
        public string UserId { get; }

        /// <summary>Gets the user's name (login).</summary>
        public string UserName { get; }

        /// <summary>Gets the user's email.</summary>
        public string Email { get; }

        /// <summary>Gets the user's phone.</summary>
        public string Phone { get; }

        /// <summary>
        /// Gets the stamp of the user data as it was fetched: a different stamp means that the user data has changed.
        /// </summary>
        public string Stamp { get; }

        /// <summary>Gets the identifiers of the permissions granted.</summary>
        public IReadOnlyCollection<Guid> Permissions { get; }

        /// <summary>Gets a value indicating whether the user has every permission, at every level.</summary>
        /// <remarks>Also true for the system identity of background work, and for a user's identity a background
        /// work item has upgraded (<see cref="IsElevated"/>).</remarks>
        public bool IsAdmin { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user has every tenant scoped permission within the identity's tenant.
        /// </summary>
        public bool IsTenantAdmin { get; }

        /// <summary>
        /// Gets a value indicating whether the user is authorized to view audit traces.
        /// </summary>
        public bool CanReadAuditTrace { get; }

        #endregion

        #region Session state

        /// <summary>Gets the ticket of the session; null for an anonymous identity or a local login.</summary>
        [JsonProperty]
        public string Ticket { get; private set; }

        /// <summary>Gets the tenant of the session; null if the user has not entered a tenant.</summary>
        [JsonProperty]
        public string TenantId { get; private set; }

        /// <summary>Gets the identifiers of the plugins assigned to the session's tenant.</summary>
        [JsonProperty]
        public IReadOnlyCollection<string> TenantPlugins { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the user has logged in, but not yet provided the second factor.
        /// </summary>
        [JsonProperty]
        public bool IsSecondFactorPending { get; private set; }

        /// <summary>
        /// Gets the data portal endpoint of the session, on a client; null for the configured (or implied local) one.
        /// Never crosses the data portal.
        /// </summary>
        [JsonIgnore]
        public DataPortalEndpoint Endpoint { get; private set; }

        /// <summary>Gets a value indicating whether the identity is anonymous (before login).</summary>
        [JsonIgnore]
        public bool IsAnonymous => null == UserId;

        /// <summary>
        /// Gets a value indicating whether the user has logged in completely (the second factor included).
        /// </summary>
        [JsonIgnore]
        public bool IsAuthenticated => !IsAnonymous && !IsSecondFactorPending;

        /// <summary>
        /// Gets a value indicating whether the identity is the system identity of background work
        /// (<see cref="Sessions.IIdentityProvider.CreateSystem"/>). Never crosses the data portal.
        /// </summary>
        [JsonIgnore]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets a value indicating whether background work has upgraded the user's identity to an administrator
        /// (<see cref="Background.RunAs.User"/> with asAdmin): <see cref="IsAdmin"/> is then true whatever the user's
        /// own rights. Never crosses the data portal.
        /// </summary>
        [JsonIgnore]
        public bool IsElevated { get; private set; }

        #endregion

        #region Permissions

        /// <summary>
        /// Gets a value indicating whether the identity holds the permission: an administrator holds every permission,
        /// a tenant administrator every tenant scoped permission within the identity's tenant.
        /// </summary>
        public bool HasPermission(Permission permission)
        {
            if (null == permission) throw new ArgumentNullException(nameof(permission));
            if (!IsAuthenticated) return false;

            return IsAdmin
                || (IsTenantAdmin && permission.IsTenantScoped && null != TenantId)
                || _permissions.Contains(permission.Id);
        }

        /// <summary>Gets a value indicating whether the identity holds any of the permissions given.</summary>
        public bool HasAnyPermission(IEnumerable<Permission> permissions)
            => null != permissions && permissions.Any(p => null != p && HasPermission(p));

        #endregion

        #region Copies

        /// <summary>
        /// Creates an exact copy of the identity, of its own type: implement as <c>new MyIdentity(this)</c> through a
        /// copy constructor calling <see cref="CarmaIdentity(CarmaIdentity)"/> and copying every member of the derived
        /// type.
        /// </summary>
        protected abstract CarmaIdentity Clone();

        /// <summary>
        /// Gets a copy of the identity for the data portal endpoint given: a client sets the endpoint the user has
        /// chosen before the identity starts a scope.
        /// </summary>
        /// <param name="endpoint">the endpoint; null for the configured (or implied local) one</param>
        public CarmaIdentity WithEndpoint(DataPortalEndpoint endpoint)
        {
            var result = CloneChecked();
            result.Endpoint = endpoint;
            return result;
        }

        /// <summary>Gets a copy of the identity with another ticket of the same session.</summary>
        internal CarmaIdentity WithTicket(string ticket)
        {
            var result = CloneChecked();
            result.Ticket = ticket;
            return result;
        }

        /// <summary>Gets a copy of the identity for a session.</summary>
        public CarmaIdentity WithSession(string ticket, string tenantId, IEnumerable<string> tenantPlugins,
            bool secondFactorPending)
        {
            var result = CloneChecked();
            result.Ticket = ticket;
            result.TenantId = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId;
            result.TenantPlugins = null == result.TenantId || null == tenantPlugins
                ? (IReadOnlyCollection<string>)Array.Empty<string>()
                : tenantPlugins.Where(p => !string.IsNullOrWhiteSpace(p))
                    .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            result.IsSecondFactorPending = secondFactorPending;
            return result;
        }

        /// <summary>Gets a copy of the identity marked as the system identity of background work.</summary>
        internal CarmaIdentity AsSystem()
        {
            var result = CloneChecked();
            result.IsSystem = true;
            result.IsAdmin = true;
            return result;
        }

        /// <summary>Gets a copy of the identity upgraded to an administrator, for background work.</summary>
        internal CarmaIdentity AsAdmin()
        {
            var result = CloneChecked();
            result.IsAdmin = true;
            result.IsElevated = true;
            return result;
        }

        private CarmaIdentity CloneChecked()
        {
            var result = Clone();
            if (null == result || result.GetType() != GetType() || ReferenceEquals(result, this))
                throw new InvalidOperationException(
                    $"{GetType().FullName}.{nameof(Clone)} must return a new instance of its own type.");
            return result;
        }

        #endregion

        /// <summary>
        /// Creates an anonymous identity: the identity of a scope before login.
        /// </summary>
        /// <param name="endpoint">the data portal endpoint of the scope (e.g. chosen on a login screen); null for the
        /// configured (or implied local) one</param>
        public static CarmaIdentity CreateAnonymous(DataPortalEndpoint endpoint = null)
            => new AnonymousIdentity { Endpoint = endpoint };

        /// <summary>
        /// Gets what makes a type unfit to be the app's identity type; empty if it is fit.
        /// </summary>
        internal static List<string> GetDefects(Type type)
        {
            var result = new List<string>();

            if (!typeof(CarmaIdentity).IsAssignableFrom(type) || type.IsAbstract || type.ContainsGenericParameters)
            {
                result.Add($"{type.FullName} is not a concrete, non-generic type deriving from {nameof(CarmaIdentity)}.");
                return result;
            }

            if (null == DataPortalContractResolver.GetSerializationConstructor(type))
                result.Add($"{type.FullName} has no constructor marked with SerializationConstructorAttribute.");

            for (var current = type; current != typeof(CarmaIdentity); current = current.BaseType)
            {
                foreach (var field in current.GetFields(BindingFlags.Instance | BindingFlags.Public
                    | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(f => !f.IsInitOnly))
                {
                    result.Add($"{current.FullName}.{field.Name} is not read-only: an identity must be immutable.");
                }
            }

            return result;
        }
    }

    /// <summary>The identity of a scope before login.</summary>
    internal sealed class AnonymousIdentity : CarmaIdentity
    {
        public AnonymousIdentity() { }

        private AnonymousIdentity(AnonymousIdentity source) : base(source) { }

        protected override CarmaIdentity Clone() => new AnonymousIdentity(this);
    }
}
