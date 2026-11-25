using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal.Protocol
{
    /// <summary>
    /// Describes a remote use case: its interface, its single remote method, and how the parameters and
    /// the result of that method cross the data portal.
    /// </summary>
    /// <remarks>
    /// <para>A generic remote method (e.g. <c>FetchAsync&lt;TLookup, TEntity&gt;(Type requester)</c> of a lookup
    /// service) is described twice: once open, as registered, with no parameters or result yet; and once for
    /// every combination of generic arguments it is called with, by <see cref="Close"/>. The generic
    /// arguments travel by name and are resolved through <see cref="RemoteUseCaseRegistry.TrustedWireTypes"/> only.</para>
    /// <para>Built lazily by <see cref="RemoteUseCaseRegistry"/>, on the first call to the use case. Only the
    /// structure is checked here; the serialization conventions are checked by
    /// <see cref="DataPortalConventions"/>.</para>
    /// </remarks>
    public sealed class RemoteUseCaseDescriptor
    {
        private readonly PropertyInfo _taskResult;
        private readonly IReadOnlyDictionary<string, Type> _trustedWireTypes;
        private readonly ConcurrentDictionary<string, RemoteUseCaseDescriptor> _closed;


        internal RemoteUseCaseDescriptor(Type interfaceType, IReadOnlyDictionary<string, Type> trustedWireTypes)
        {
            InterfaceType = interfaceType ?? throw new ArgumentNullException(nameof(interfaceType));
            _trustedWireTypes = trustedWireTypes ?? throw new ArgumentNullException(nameof(trustedWireTypes));

            if (!interfaceType.IsInterface || interfaceType.ContainsGenericParameters)
                throw new NotSupportedException($"Use case {interfaceType} cannot be invoked remotely: " +
                    "a closed interface type is required.");

            Name = GetName(interfaceType);
            Method = FindRemoteMethod(interfaceType);
            GenericArguments = Type.EmptyTypes;

            if (Method.IsGenericMethodDefinition)
            {
                IsGenericDefinition = true;
                GenericArity = Method.GetGenericArguments().Length;
                _closed = new ConcurrentDictionary<string, RemoteUseCaseDescriptor>(StringComparer.Ordinal);
                return;
            }

            (Parameters, ParameterTypeNames, Result, _taskResult, AcceptsCancellation) = DescribeSignature(Method, Name);
        }

        private RemoteUseCaseDescriptor(RemoteUseCaseDescriptor definition, Type[] genericArguments,
            MethodInfo closedMethod)
        {
            InterfaceType = definition.InterfaceType;
            _trustedWireTypes = definition._trustedWireTypes;
            Name = definition.Name;
            Method = closedMethod;
            GenericArguments = genericArguments;
            GenericArity = genericArguments.Length;

            (Parameters, ParameterTypeNames, Result, _taskResult, AcceptsCancellation) = DescribeSignature(Method, Name);
        }


        /// <summary>Gets the use case interface.</summary>
        public Type InterfaceType { get; }

        /// <summary>Gets a name by which the use case is identified on the wire.</summary>
        public string Name { get; }

        /// <summary>Gets the single remote method of the use case; for a closed generic method, closed over
        /// <see cref="GenericArguments"/>.</summary>
        public MethodInfo Method { get; }

        /// <summary>Gets a value indicating whether this describes an open generic remote method, which must
        /// be closed (<see cref="Close"/>) before it is called; its parameters and result are not described.</summary>
        public bool IsGenericDefinition { get; }

        /// <summary>Gets the number of generic arguments of the remote method; 0 if it is not generic.</summary>
        public int GenericArity { get; }

        /// <summary>Gets the generic arguments a generic remote method is closed over; empty otherwise.</summary>
        public IReadOnlyList<Type> GenericArguments { get; }

        /// <summary>Gets the wire names of <see cref="GenericArguments"/>, as a call carries them
        /// (<see cref="DataPortalCall.GenericArguments"/>).</summary>
        public IReadOnlyList<string> GenericArgumentNames => GenericArguments.Select(GetName).ToList();

        /// <summary>Gets the descriptors of the remote method parameters; null for an open generic method.</summary>
        public IReadOnlyList<RemoteValueDescriptor> Parameters { get; }

        /// <summary>Gets a descriptor of the remote method result; null if it returns nothing, or for an
        /// open generic method.</summary>
        public RemoteValueDescriptor Result { get; }

        /// <summary>
        /// Gets the wire type names of the parameters, in signature order, as a call carries them
        /// (<see cref="DataPortalCall.ParameterTypes"/>); null for an open generic method.
        /// </summary>
        public IReadOnlyList<string> ParameterTypeNames { get; }

        /// <summary>Gets a value indicating whether the remote method takes a CancellationToken as its last parameter:
        /// it does not change state, so a call may be cancelled. The token is not a wire parameter.</summary>
        public bool AcceptsCancellation { get; }


        /// <summary>
        /// Gets the descriptor of a generic remote method closed over the generic arguments given; for a
        /// method that is not generic, this descriptor if no arguments are given.
        /// </summary>
        /// <param name="genericArguments">the generic arguments, each of them a trusted wire type
        /// (<see cref="RemoteUseCaseRegistry.TrustedWireTypes"/>)</param>
        /// <exception cref="NotSupportedException">the number of arguments is wrong, an argument is not a
        /// trusted wire type, or the arguments violate the method's constraints</exception>
        public RemoteUseCaseDescriptor Close(IReadOnlyList<Type> genericArguments)
        {
            genericArguments = genericArguments ?? Type.EmptyTypes;

            if (!IsGenericDefinition)
            {
                if (genericArguments.Count > 0 && !genericArguments.SequenceEqual(GenericArguments))
                    throw new NotSupportedException($"Remote method {Name}.{Method.Name} is not generic.");
                return this;
            }

            if (genericArguments.Count != GenericArity) throw new NotSupportedException(
                $"Remote method {Name}.{Method.Name} takes {GenericArity} generic argument(s), {genericArguments.Count} given.");

            foreach (var argument in genericArguments)
            {
                // the same check on both sides: a type that the server would not resolve is refused before sending
                if (!(_trustedWireTypes.TryGetValue(GetName(argument), out var known) && known == argument)) throw new NotSupportedException(
                    $"{argument} is not a trusted wire type, so it cannot be a generic argument of a remote method; " +
                    "only lookup types, their entity types and use case implementations can.");
            }

            var key = string.Join("|", genericArguments.Select(GetName));

            return _closed.GetOrAdd(key, _ =>
            {
                MethodInfo closed;
                try
                {
                    closed = Method.MakeGenericMethod(genericArguments.ToArray());
                }
                catch (ArgumentException ex)
                {
                    throw new NotSupportedException(
                        $"The generic arguments {key} violate the constraints of {Name}.{Method.Name}.", ex);
                }

                return new RemoteUseCaseDescriptor(this, genericArguments.ToArray(), closed);
            });
        }

        /// <summary>
        /// Resolves generic argument names received in a call and closes the method over them.
        /// </summary>
        /// <exception cref="NotSupportedException">a name is not a trusted wire type, or the arguments don't fit</exception>
        internal RemoteUseCaseDescriptor CloseByNames(IReadOnlyList<string> genericArgumentNames)
        {
            var arguments = new Type[genericArgumentNames?.Count ?? 0];
            for (var i = 0; i < arguments.Length; i++)
            {
                var name = genericArgumentNames[i];
                if (null == name || !_trustedWireTypes.TryGetValue(name, out arguments[i])) throw new NotSupportedException(
                    $"Generic argument {i} of {Name} is not a trusted wire type.");
            }

            return Close(arguments);
        }


        /// <summary>
        /// Gets the wire name of a type: a use case interface, a generic argument or a <see cref="Type"/> value.
        /// <see cref="Type.ToString"/>, without assembly name or version, so that different builds of the client
        /// and the server match. The one place where the rule is written.
        /// </summary>
        internal static string GetName(Type type) => (type ?? throw new ArgumentNullException(nameof(type))).ToString();

        /// <summary>
        /// Finds the single remote method of a use case interface, including the interfaces it extends.
        /// </summary>
        /// <exception cref="NotSupportedException">there is not exactly one</exception>
        internal static MethodInfo FindRemoteMethod(Type interfaceType)
        {
            var methods = new[] { interfaceType }.Concat(interfaceType.GetInterfaces())
                .SelectMany(i => i.GetMethods())
                .Where(m => m.IsDefined(typeof(RemoteMethodAttribute), true))
                .ToList();

            if (methods.Count != 1) throw new NotSupportedException(
                $"Use case {interfaceType} cannot be invoked remotely: exactly one [RemoteMethod] is required, " +
                $"{methods.Count} found.");

            var method = methods[0];
            var returnType = method.ReturnType;

            if (returnType != typeof(Task) && !(returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>)))
                throw new NotSupportedException(
                    $"Use case {interfaceType} cannot be invoked remotely: its remote method must return Task or Task<TResult>.");

            return method;
        }

        private static (IReadOnlyList<RemoteValueDescriptor>, IReadOnlyList<string>, RemoteValueDescriptor, PropertyInfo, bool)
            DescribeSignature(MethodInfo method, string name)
        {
            try
            {
                var all = method.GetParameters();
                // a trailing CancellationToken is not wire data; anywhere else it is a mistake
                var acceptsCancellation = all.Length > 0 && all[all.Length - 1].ParameterType == typeof(CancellationToken);
                var wire = acceptsCancellation ? all.Take(all.Length - 1).ToArray() : all;
                if (wire.Any(p => p.ParameterType == typeof(CancellationToken))) throw new NotSupportedException(
                    $"a {nameof(CancellationToken)} can only be the last parameter");

                var parameters = wire.Select(p => new RemoteValueDescriptor(p.ParameterType)).ToList();

                var returnType = method.ReturnType;
                if (returnType == typeof(Task)) return (parameters, parameters.Select(p => p.WireTypeName).ToList(),
                    null, null, acceptsCancellation);

                return (parameters, parameters.Select(p => p.WireTypeName).ToList(),
                    new RemoteValueDescriptor(returnType.GetGenericArguments()[0]),
                    returnType.GetProperty(nameof(Task<object>.Result)), acceptsCancellation);
            }
            catch (NotSupportedException ex)
            {
                throw new NotSupportedException($"Use case {name} cannot be invoked remotely: {ex.Message}.", ex);
            }
        }


        /// <summary>
        /// Gets a result of a completed task returned by the remote method.
        /// </summary>
        internal object GetResult(Task completedTask)
            => _taskResult?.GetValue(completedTask);
    }
}
