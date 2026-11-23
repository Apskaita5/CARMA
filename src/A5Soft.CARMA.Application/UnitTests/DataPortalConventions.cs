using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using A5Soft.CARMA.Application.DataPortal.Protocol;
using A5Soft.CARMA.Application.IoC;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Files;
using A5Soft.CARMA.Domain.UnitTests;
using Newtonsoft.Json.Serialization;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// Checks the data portal conventions in depth: the signature of every remote method, and the whole
    /// type graph of its parameters and result, as the data portal will actually serialize it.
    /// </summary>
    /// <remarks>
    /// <para>Nothing checks this at startup: the registry builds descriptors lazily and checks only their
    /// structure. Run it:</para>
    /// <list type="bullet">
    /// <item><see cref="Validate(Assembly)"/> in a unit test, over every assembly declaring or implementing
    /// use cases, so that a breach fails the build. It walks every use case and lookup type in the assembly;
    /// with 10–20k use cases expect tens of seconds.</item>
    /// <item><see cref="Validate(Type)"/> for one use case, e.g. a plugin's at startup;
    /// <see cref="IoC.IoCManagerBase"/> does it for every use case and lookup type of a plugin.</item>
    /// </list>
    /// <para>The rules, at any depth of a parameter or a result:</para>
    /// <list type="bullet">
    /// <item>a use case has exactly one [RemoteMethod], returning <c>Task</c> or <c>Task&lt;T&gt;</c>;</item>
    /// <item>a domain object (<see cref="IDtoSerializable{TDto}"/>) is allowed only as a top-level parameter
    /// or result, where it travels as its DTO; nested in a value (a POCO, a DTO, a list) it is refused;</item>
    /// <item>no abstract class or interface, except the collection interfaces Json.NET creates a list or a
    /// dictionary for (<c>IEnumerable&lt;T&gt;</c>, <c>IList&lt;T&gt;</c>, <c>IReadOnlyDictionary&lt;K,V&gt;</c>, …);</item>
    /// <item>no untyped member: <see cref="object"/>, <c>JToken</c>, non-generic collections;</item>
    /// <item>no <see cref="Stream"/>: a file is a <see cref="FileContent"/>;</item>
    /// <item>a lookup or a query result has a [SerializationConstructor]; any other class has one or a public
    /// parameterless constructor (or a single public constructor, which Json.NET uses);</item>
    /// <item>a serialization constructor is private, and each of its parameters matches a serialized
    /// property of the same name and type;</item>
    /// <item>a dictionary key is a string, an enum or a primitive-like value (enforced at run time as well, by the
    /// contract resolver).</item>
    /// </list>
    /// <para>A <see cref="Type"/> value is allowed: it is resolved through the trusted wire types at run time. For a
    /// generic remote method, the parameters that depend on its generic arguments are checked through the
    /// lookup types instead, which are validated with the assembly.</para>
    /// <para>Each run uses its own contract resolver, discarded afterwards, so validation keeps nothing in
    /// memory.</para>
    /// </remarks>
    public static class DataPortalConventions
    {
        /// <summary>
        /// Checks one use case. Meant for plugin use cases at startup.
        /// </summary>
        /// <param name="useCaseType">a use case interface (marked with [UseCase] or [RemoteService]) or a class
        /// implementing exactly one</param>
        /// <returns>every violation found; empty if none</returns>
        public static IReadOnlyList<ConventionViolation> Validate(Type useCaseType)
        {
            if (null == useCaseType) throw new ArgumentNullException(nameof(useCaseType));

            var violations = new List<ConventionViolation>();
            var interfaceType = FindRemoteInterface(useCaseType, violations);
            if (null != interfaceType) new Walker(violations).ValidateUseCase(interfaceType);

            return violations;
        }

        /// <summary>
        /// Checks every use case and lookup type of an assembly, and throws if any breaches the conventions.
        /// </summary>
        /// <exception cref="InvalidOperationException">the conventions are breached; the message lists the
        /// violations (<see cref="Validate(Assembly)"/> returns them as a list)</exception>
        public static void EnsureValid(Assembly assembly)
        {
            var violations = Validate(assembly);
            if (violations.Count > 0) throw new InvalidOperationException(
                ConventionViolation.Format($"Assembly {assembly.GetName().Name}", violations));
        }

        /// <summary>
        /// Checks one use case, and throws if it breaches the conventions.
        /// </summary>
        /// <exception cref="InvalidOperationException">the conventions are breached; the message lists the
        /// violations (<see cref="Validate(Type)"/> returns them as a list)</exception>
        public static void EnsureValid(Type useCaseType)
        {
            var violations = Validate(useCaseType);
            if (violations.Count > 0) throw new InvalidOperationException(
                ConventionViolation.Format($"Use case {useCaseType}", violations));
        }


        /// <summary>
        /// Checks every use case and lookup type of an assembly against the data portal conventions, and that its
        /// singleton services take only singletons. Meant for unit tests.
        /// </summary>
        public static IReadOnlyList<ConventionViolation> Validate(Assembly assembly)
        {
            if (null == assembly) throw new ArgumentNullException(nameof(assembly));

            var scan = AssemblyScan.Of(assembly);
            var violations = Validate(scan);
            ValidateSingletonDependencies(scan, violations);   // unit tests only: not part of plugin validation at startup
            return violations;
        }


        /// <summary>
        /// Checks every use case and lookup type of a scan. Names are unique within a scan; across scans the
        /// merge checks them.
        /// </summary>
        internal static List<ConventionViolation> Validate(AssemblyScan scan)
        {
            var violations = new List<ConventionViolation>();
            var walker = new Walker(violations);

            foreach (var useCase in scan.RemoteServices.Values) walker.ValidateUseCase(useCase);
            // the lookups declared in the scanned assembly are among its trusted wire types (with their entity
            // types, which are skipped here, as they are not lookups)
            foreach (var type in scan.TrustedWireTypes.Values)
                if (null != AssemblyScan.GetLookupEntityType(type)) walker.ValidateLookup(type);

            return violations;
        }

        /// <summary>
        /// A singleton should take only singletons: a scoped or transient service it takes is created once and kept for
        /// the life of the app (the container does not report a transient one). Checks every public constructor of the
        /// singleton services and health checks implemented in the assembly; a parameter whose type has no
        /// <see cref="ServiceAttribute"/> (e.g. an external service) is not checked, as its lifetime is not known here.
        /// </summary>
        private static void ValidateSingletonDependencies(AssemblyScan scan, List<ConventionViolation> violations)
        {
            var singletons = scan.ServiceImplementations.Concat(scan.HealthChecks)
                .Where(s => s.Lifetime == ServiceLifetime.Singleton && null == s.ImplementationInstance);

            foreach (var service in singletons)
                foreach (var constructor in service.ImplementationType.GetConstructors())
                    foreach (var parameter in constructor.GetParameters())
                    {
                        var type = parameter.ParameterType;
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                            type = type.GetGenericArguments()[0];

                        var lifetime = type.GetCustomAttribute<ServiceAttribute>()?.Lifetime;
                        if (null != lifetime && lifetime != ServiceLifetime.Singleton)
                            violations.Add(new ConventionViolation(service.ImplementationType,
                                $"constructor parameter {parameter.Name}",
                                $"a singleton takes the {lifetime.Value.ToString().ToLowerInvariant()} service {type}, " +
                                "which is then kept for the life of the app"));
                    }
        }


        private static Type FindRemoteInterface(Type useCaseType, List<ConventionViolation> violations)
        {
            if (useCaseType.IsInterface)
            {
                if (useCaseType.IsDefined(typeof(RemoteServiceAttribute), false)) return useCaseType;

                violations.Add(new ConventionViolation(useCaseType, string.Empty,
                    "is not marked with [UseCase] or [RemoteService]"));
                return null;
            }

            var candidates = useCaseType.GetInterfaces()
                .Where(i => i.IsDefined(typeof(RemoteServiceAttribute), false))
                .ToList();
            candidates = candidates.Where(c => !candidates.Any(o => o != c && c.IsAssignableFrom(o))).ToList();

            if (candidates.Count == 1) return candidates[0];

            violations.Add(new ConventionViolation(useCaseType, string.Empty, candidates.Count == 0
                ? "does not implement a use case interface"
                : "implements more than one use case interface"));
            return null;
        }


        /// <summary>
        /// Walks type graphs with its own contract resolver, so that the walk is exactly what the data
        /// portal will serialize (ignored members, converters, DTOs) and nothing is cached afterwards.
        /// </summary>
        private sealed class Walker
        {
            private readonly List<ConventionViolation> _violations;
            private readonly DataPortalContractResolver _resolver = new DataPortalContractResolver();
            private Type _subject;
            private HashSet<Type> _visited;


            public Walker(List<ConventionViolation> violations) => _violations = violations;


            public void ValidateUseCase(Type interfaceType)
            {
                Begin(interfaceType);

                if (interfaceType.ContainsGenericParameters)
                {
                    Fail(string.Empty, "a use case interface cannot be generic; a generic remote method can");
                    return;
                }

                MethodInfo method;
                try
                {
                    method = RemoteUseCaseDescriptor.FindRemoteMethod(interfaceType);
                }
                catch (NotSupportedException ex)
                {
                    Fail(string.Empty, ex.Message);
                    return;
                }

                var parameters = method.GetParameters();
                for (var i = 0; i < parameters.Length; i++)
                {
                    var type = parameters[i].ParameterType;
                    var path = $"parameter {i} ({parameters[i].Name})";

                    if (type.IsByRef) Fail(path, "ref and out parameters cannot cross the data portal");
                    else if (!type.ContainsGenericParameters) ValidateValue(type, path, true);
                }

                if (method.ReturnType.IsGenericType)
                {
                    var resultType = method.ReturnType.GetGenericArguments()[0];
                    if (!resultType.ContainsGenericParameters) ValidateValue(resultType, "result", true);
                }
            }

            public void ValidateLookup(Type lookupType)
            {
                Begin(lookupType);
                ValidateValue(lookupType, "lookup", false);
            }


            private void Begin(Type subject)
            {
                _subject = subject;
                _visited = new HashSet<Type>();
            }

            private void Fail(string path, string message)
                => _violations.Add(new ConventionViolation(_subject, path, message));

            private void ValidateValue(Type type, string path, bool topLevel)
            {
                type = Nullable.GetUnderlyingType(type) ?? type;

                if (typeof(FileContent).IsAssignableFrom(type) || type == typeof(Type)) return;

                if (typeof(Stream).IsAssignableFrom(type))
                {
                    Fail(path, $"a {nameof(Stream)} cannot cross the data portal, use {nameof(FileContent)} instead");
                    return;
                }

                if (type.ContainsGenericParameters) return;

                // domain objects: as their DTO at the top level only
                Type dtoType;
                try
                {
                    dtoType = RemoteValueDescriptor.FindDtoType(type);
                }
                catch (NotSupportedException ex)
                {
                    Fail(path, ex.Message);
                    return;
                }

                // a rich domain object: state tracking and binding; lookups (which are IDomainObject too) are values
                if (null != dtoType || typeof(ITrackState).IsAssignableFrom(type) || typeof(BindableBase).IsAssignableFrom(type))
                {
                    if (!topLevel)
                    {
                        Fail(path, $"domain object {type} is nested in a value; only a top-level parameter or " +
                            "result travels as its DTO");
                        return;
                    }

                    if (null == dtoType)
                    {
                        Fail(path, $"domain object {type} does not implement IDtoSerializable<TDto>, so it cannot cross the data portal");
                        return;
                    }

                    path += $" (as {dtoType.Name})";
                    type = dtoType;
                }

                if (!_visited.Add(type)) return;

                if (type == typeof(object))
                {
                    Fail(path, "an untyped (object) value cannot be deserialized to what was sent");
                    return;
                }

                JsonContract contract;
                try
                {
                    contract = _resolver.ResolveContract(type);
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException
                    || ex is Newtonsoft.Json.JsonException)
                {
                    // e.g. a serialization constructor that is not private, or whose parameters don't match
                    Fail(path, ex.Message);
                    return;
                }

                // a converter decides the JSON itself (e.g. an entity identity)
                if (null != contract.Converter) return;

                switch (contract)
                {
                    case JsonPrimitiveContract _:   // JsonStringContract (a type converter to string) included
                        return;

                    case JsonLinqContract _:
                        Fail(path, "untyped JSON (JToken) is not allowed; declare a type");
                        return;

                    case JsonArrayContract array:
                        if (null == array.CollectionItemType)
                        {
                            Fail(path, "a non-generic collection is untyped; use a generic one");
                            return;
                        }
                        if (array.CreatedType.IsAbstract || array.CreatedType.IsInterface)
                        {
                            Fail(path, $"collection type {type} cannot be created; use an array, List<T> or a generic collection interface");
                            return;
                        }
                        ValidateValue(array.CollectionItemType, path + "[]", false);
                        return;

                    case JsonDictionaryContract dictionary:
                        if (null == dictionary.DictionaryKeyType || null == dictionary.DictionaryValueType)
                        {
                            Fail(path, "a non-generic dictionary is untyped; use a generic one");
                            return;
                        }
                        if (dictionary.CreatedType.IsAbstract || dictionary.CreatedType.IsInterface)
                        {
                            Fail(path, $"dictionary type {type} cannot be created; use Dictionary<TKey, TValue> or a generic dictionary interface");
                            return;
                        }
                        // the key type is checked by the contract resolver itself, which refused the contract above
                        ValidateValue(dictionary.DictionaryValueType, path + "{}", false);
                        return;

                    case JsonObjectContract obj:
                        ValidateObject(type, obj, path);
                        return;

                    default:
                        Fail(path, $"type {type} cannot cross the data portal ({contract.GetType().Name})");
                        return;
                }
            }

            private void ValidateObject(Type type, JsonObjectContract contract, string path)
            {
                if (type.IsInterface || type.IsAbstract)
                {
                    Fail(path, $"{(type.IsInterface ? "interface" : "abstract class")} {type} cannot be " +
                        "deserialized; declare a concrete type");
                    return;
                }

                var constructor = DataPortalContractResolver.GetSerializationConstructor(type);

                var isLookup = null != AssemblyScan.GetLookupEntityType(type);
                if (null == constructor && (isLookup || typeof(QueryResultBase).IsAssignableFrom(type)))
                {
                    Fail(path, $"{(isLookup ? "lookup" : "query result")} {type} has no constructor marked with " +
                        "[SerializationConstructor]");
                }
                else if (null == constructor && !type.IsValueType && !CanCreate(type))
                {
                    Fail(path, $"{type} has neither a public parameterless constructor nor a [SerializationConstructor]");
                }

                foreach (var property in contract.Properties.Where(p => !p.Ignored && null != p.PropertyType))
                {
                    ValidateValue(property.PropertyType, $"{path}.{property.PropertyName}", false);
                }
            }

            /// <summary>
            /// Whether Json.NET can create the type without a serialization constructor: a public parameterless
            /// constructor, or a single public constructor (which it then uses).
            /// </summary>
            private static bool CanCreate(Type type)
            {
                var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
                return constructors.Any(c => c.GetParameters().Length == 0) || constructors.Length == 1;
            }
        }
    }
}
