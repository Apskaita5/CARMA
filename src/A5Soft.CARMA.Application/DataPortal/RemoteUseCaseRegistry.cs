using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using A5Soft.CARMA.Application.IoC;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A registry of the use cases (remote services) that can be invoked through the data portal, and of the
    /// types that may be named on the wire. It is the only place where a name that came over the wire is turned
    /// into a type.
    /// </summary>
    /// <remarks>
    /// <para>Immutable: built once, from the merged assembly scans, by <see cref="IoC.IoCManagerBase"/> (which adds
    /// it to the container) or by <see cref="FromAssemblies"/>. Nothing can add a use case or a trusted wire type
    /// afterwards, so what can be named on the wire is exactly what the scan found. Thread safe.</para>
    /// <para>A use case is recorded by name only. Its descriptor (the reflection over its remote method and the
    /// values crossing the portal) is built on its first call and kept, so an application with 10–20k use cases
    /// starts quickly and holds descriptors only for the use cases it actually serves. A use case that cannot be
    /// described fails on each of its calls, with the reason.</para>
    /// <para>Nothing here validates the serialization conventions in depth: that is
    /// <see cref="DataPortalConventions"/>, run in unit tests and on plugins.</para>
    /// </remarks>
    public sealed class RemoteUseCaseRegistry
    {
        private readonly Dictionary<string, Type> _useCases;
        private readonly Dictionary<string, Type> _trustedWireTypes;
        private readonly HashSet<Type> _useCaseInterfaces;
        private readonly ConcurrentDictionary<Type, Lazy<RemoteUseCaseDescriptor>> _descriptors
            = new ConcurrentDictionary<Type, Lazy<RemoteUseCaseDescriptor>>();
        private readonly ConcurrentDictionary<Type, Type> _interfaceByType
            = new ConcurrentDictionary<Type, Type>();


        /// <summary>
        /// Creates a registry from merged scans; the dictionaries are copied.
        /// </summary>
        /// <param name="useCases">the remote service interfaces, by their wire names</param>
        /// <param name="trustedWireTypes">the types that may be named on the wire, by their wire names</param>
        internal RemoteUseCaseRegistry(IReadOnlyDictionary<string, Type> useCases,
            IReadOnlyDictionary<string, Type> trustedWireTypes)
        {
            if (null == useCases) throw new ArgumentNullException(nameof(useCases));
            if (null == trustedWireTypes) throw new ArgumentNullException(nameof(trustedWireTypes));

            _useCases = useCases.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);
            _trustedWireTypes = trustedWireTypes.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);
            _useCaseInterfaces = new HashSet<Type>(_useCases.Values);
        }


        /// <summary>
        /// Gets the use case (remote service) interfaces, by their wire names.
        /// </summary>
        public IReadOnlyDictionary<string, Type> UseCases => _useCases;

        /// <summary>
        /// Gets the only types that can be named on the wire, by their wire names: as the generic arguments of a
        /// generic remote method, or as a <see cref="Type"/> value in a parameter or a result. Everything else
        /// that crosses the data portal is described by the remote method signature, never by a name the other
        /// side has sent.
        /// </summary>
        /// <remarks>
        /// <para>Trusted to move by wire: every lookup type (deriving from <see cref="Domain.LookupBase{T}"/>) with its
        /// entity type, every concrete domain object (deriving from <see cref="Domain.DomainObject{T}"/>, entities
        /// included), and every implementation of a remote use case (e.g. the requester type of a lookup call,
        /// which must be a use case class). Use case interfaces are not here: they travel only as use case names,
        /// resolved through <see cref="UseCases"/>. A wire name is <see cref="Type.ToString"/>:
        /// no assembly name or version, so different builds of the client and the server match, as they do for
        /// use case names.</para>
        /// <para>Resolving a name never loads an assembly, runs a type initializer or reaches a type outside the
        /// list: an unknown name is simply not found, and the call is refused. To check a type, compare the type
        /// found by its name as well, as another type may have the same full name in another assembly.</para>
        /// </remarks>
        public IReadOnlyDictionary<string, Type> TrustedWireTypes => _trustedWireTypes;


        /// <summary>
        /// Creates a registry for the remote services implemented in the assemblies given, and their trusted wire types.
        /// </summary>
        /// <exception cref="InvalidOperationException">two different use cases or trusted wire types have the same name</exception>
        public static RemoteUseCaseRegistry FromAssemblies(params Assembly[] assemblies)
        {
            if (null == assemblies) throw new ArgumentNullException(nameof(assemblies));

            return FromScans(assemblies.Where(a => null != a).Distinct().Select(AssemblyScan.Of));
        }

        /// <summary>
        /// Creates a registry from scans, merged.
        /// </summary>
        /// <exception cref="InvalidOperationException">two different use cases or trusted wire types have the same name</exception>
        internal static RemoteUseCaseRegistry FromScans(IEnumerable<AssemblyScan> scans)
        {
            var useCases = new Dictionary<string, Type>(StringComparer.Ordinal);
            var trustedWireTypes = new Dictionary<string, Type>(StringComparer.Ordinal);
            var clashes = new List<string>();

            foreach (var scan in scans)
            {
                Merge(useCases, scan.RemoteServices, (existing, type) => clashes.Add(ClashMessage("use cases", existing, type)));
                Merge(trustedWireTypes, scan.TrustedWireTypes, (existing, type) => clashes.Add(ClashMessage("types", existing, type)));
            }

            if (clashes.Count > 0) throw new InvalidOperationException(string.Join(" ", clashes.Distinct()));

            return new RemoteUseCaseRegistry(useCases, trustedWireTypes);
        }

        /// <summary>
        /// Merges types by wire name into a dictionary: the same type again is fine (e.g. an entity type referenced
        /// by lookups in two assemblies); a different type with a name already taken is reported and not merged.
        /// </summary>
        /// <param name="into">the dictionary to merge into</param>
        /// <param name="from">the types to merge</param>
        /// <param name="onClash">called with the type already there and the other type with its name</param>
        internal static void Merge(Dictionary<string, Type> into, IReadOnlyDictionary<string, Type> from,
            Action<Type, Type> onClash)
        {
            foreach (var pair in from)
            {
                if (into.TryGetValue(pair.Key, out var existing))
                {
                    if (existing != pair.Value) onClash(existing, pair.Value);
                    continue;
                }

                into.Add(pair.Key, pair.Value);
            }
        }

        internal static string ClashMessage(string what, Type existing, Type type)
            => $"The {what} {existing.AssemblyQualifiedName} and {type.AssemblyQualifiedName} have the same name, " +
               "so the data portal cannot tell them apart.";


        /// <summary>
        /// Server side: finds a use case by the name received in a remote call. For a generic remote method,
        /// the descriptor is the open one: close it with <see cref="RemoteUseCaseDescriptor.Close"/>.
        /// </summary>
        /// <returns>false if no use case has the name</returns>
        /// <exception cref="NotSupportedException">the use case is registered but cannot be invoked remotely
        /// (e.g. it has no [RemoteMethod]); the message says why</exception>
        public bool TryGet(string name, out RemoteUseCaseDescriptor descriptor)
        {
            descriptor = null;
            if (null == name || !_useCases.TryGetValue(name, out var interfaceType)) return false;

            descriptor = GetDescriptor(interfaceType);
            return true;
        }

        /// <summary>
        /// Client side: gets a descriptor for a use case interface or for a class implementing one.
        /// </summary>
        /// <param name="useCaseType">a use case interface or implementation type</param>
        /// <exception cref="InvalidOperationException">the type is not, or does not implement, exactly one
        /// registered use case interface</exception>
        /// <exception cref="NotSupportedException">the use case cannot be invoked remotely; the message says why</exception>
        public RemoteUseCaseDescriptor GetFor(Type useCaseType)
        {
            if (null == useCaseType) throw new ArgumentNullException(nameof(useCaseType));

            return GetDescriptor(_interfaceByType.GetOrAdd(useCaseType, FindInterface));
        }


        private RemoteUseCaseDescriptor GetDescriptor(Type interfaceType)
        {
            // Lazy<T> in its default mode caches an exception too: an unsupported use case fails the same
            // way on every call, without repeating the reflection
            return _descriptors.GetOrAdd(interfaceType, t => new Lazy<RemoteUseCaseDescriptor>(
                () => new RemoteUseCaseDescriptor(t, _trustedWireTypes))).Value;
        }

        private Type FindInterface(Type useCaseType)
        {
            if (_useCaseInterfaces.Contains(useCaseType)) return useCaseType;

            var candidates = useCaseType.GetInterfaces().Where(_useCaseInterfaces.Contains).ToList();

            // an interface that only extends another remote interface is not a second use case
            candidates = candidates
                .Where(c => !candidates.Any(o => o != c && c.IsAssignableFrom(o)))
                .ToList();

            if (candidates.Count == 1) return candidates[0];

            throw new InvalidOperationException(candidates.Count == 0
                ? $"Type {useCaseType} does not implement a registered remote use case interface."
                : $"Type {useCaseType} implements more than one remote use case interface.");
        }
    }
}
