using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Files;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace A5Soft.CARMA.Application.DataPortal.Protocol
{
    /// <summary>
    /// Describes how a parameter or a result of a remote method crosses the data portal. It is
    /// built from the method signature only, and it is the same on both sides, so neither side
    /// ever needs a type name that the other side has sent.
    /// </summary>
    /// <remarks>
    /// A value crosses in one of three ways:
    /// <list type="bullet">
    /// <item>a domain object (<see cref="IDtoSerializable{TDto}"/>) as its DTO, rebuilt on arrival
    /// with the local <see cref="IApplicationContext"/>;</item>
    /// <item>a <see cref="FileContent"/>, anywhere in a parameter (top-level, in a DTO, in a list),
    /// as a reference in the JSON and its content as a multipart part; a <see cref="FileContent"/>
    /// result as a streamed download, while a file nested in a result is always sent as null;</item>
    /// <item>anything else (POCOs, query results, lookups, primitives, lists) as JSON, read-only
    /// types through their serialization constructor.</item>
    /// </list>
    /// </remarks>
    public sealed class RemoteValueDescriptor
    {
        internal RemoteValueDescriptor(Type declaredType)
        {
            DeclaredType = declaredType ?? throw new ArgumentNullException(nameof(declaredType));

            if (declaredType.IsByRef || declaredType.IsPointer || declaredType.ContainsGenericParameters)
                throw new NotSupportedException($"type {declaredType} cannot cross the data portal");

            if (typeof(Stream).IsAssignableFrom(declaredType)) throw new NotSupportedException(
                $"a {nameof(Stream)} cannot cross the data portal, use {nameof(FileContent)} instead");

            IsFile = declaredType == typeof(FileContent);

            var dtoType = IsFile ? null : FindDtoType(declaredType);

            IsEntity = null != dtoType;
            WireType = dtoType ?? declaredType;
            WireTypeName = WireType.ToString();
            AllowsNull = !declaredType.IsValueType || null != Nullable.GetUnderlyingType(declaredType);
        }


        /// <summary>Gets a type declared in the remote method signature.</summary>
        public Type DeclaredType { get; }

        /// <summary>
        /// Gets a type that is actually serialized: the DTO type for a domain object,
        /// otherwise the declared type.
        /// </summary>
        public Type WireType { get; }

        /// <summary>
        /// Gets a name of the wire type. No assembly name or version is included, so different
        /// builds of the client and the server still match.
        /// </summary>
        public string WireTypeName { get; }

        /// <summary>
        /// Gets a value indicating whether the declared type is a domain object that crosses the
        /// data portal as its DTO.
        /// </summary>
        public bool IsEntity { get; }

        /// <summary>
        /// Gets a value indicating whether the declared type is <see cref="FileContent"/>: a file
        /// reference as a parameter, a streamed download as a result.
        /// </summary>
        public bool IsFile { get; }

        /// <summary>Gets a value indicating whether the value can be null.</summary>
        public bool AllowsNull { get; }


        /// <summary>
        /// Writes a value as JSON: a domain object is replaced by its DTO, and files are handled by
        /// the <see cref="Internal.FileContentConverter"/> according to the serializer's context.
        /// </summary>
        /// <param name="writer">a writer to write to</param>
        /// <param name="serializer">the data portal serializer</param>
        /// <param name="value">a value to write</param>
        internal void Write(JsonWriter writer, JsonSerializer serializer, object value)
        {
            if (null == value)
            {
                if (!AllowsNull) throw new ArgumentNullException(nameof(value),
                    $"A value of type {DeclaredType} cannot be null.");

                writer.WriteNull();
                return;
            }

            if (!DeclaredType.IsInstanceOfType(value)) throw new ArgumentException(
                $"A value of type {value.GetType()} was given where {DeclaredType} is declared.",
                nameof(value));

            if (IsEntity) value = ((IDtoSerializable<IDomainObjectDto>)value).ToDto();

            serializer.Serialize(writer, value, WireType);
        }

        /// <summary>
        /// Reads a value from JSON, checks it against the signature and, for a domain object,
        /// builds it from its DTO.
        /// </summary>
        /// <param name="reader">a reader positioned on the first token of the value</param>
        /// <param name="serializer">the data portal serializer</param>
        /// <param name="context">the local application context, for the domain objects built</param>
        /// <exception cref="RemoteCallRejectedException">the value does not match the signature</exception>
        internal object Read(JsonReader reader, JsonSerializer serializer, Domain.IDomainContext context)
        {
            object result;
            try
            {
                result = reader.TokenType == JsonToken.Null
                    ? null
                    : serializer.Deserialize(reader, WireType);

                if (IsEntity && null != result) result = ((IDomainObjectDto)result).ToEntity(context);
            }
            catch (RemoteCallRejectedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw Rejected("the value cannot be deserialized", ex);
            }

            if (null == result && !AllowsNull) throw Rejected("the value cannot be null");

            if (null != result && !DeclaredType.IsInstanceOfType(result))
                throw Rejected($"the value deserialized is a {result.GetType()}");

            return result;
        }


        private RemoteCallRejectedException Rejected(string reason, Exception inner = null)
            => new RemoteCallRejectedException(RemoteErrorKind.BadRequest,
                $"A value of type {DeclaredType} was rejected: {reason}.", inner);

        internal static Type FindDtoType(Type declaredType)
        {
            var interfaces = declaredType.IsInterface
                ? new[] { declaredType }.Concat(declaredType.GetInterfaces())
                : declaredType.GetInterfaces();

            var dtoTypes = interfaces
                .Where(i => i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IDtoSerializable<>))
                .Select(i => i.GetGenericArguments()[0])
                .Distinct()
                .ToList();

            if (dtoTypes.Count > 1) throw new NotSupportedException(
                $"type {declaredType} implements IDtoSerializable<> for more than one DTO type");

            return dtoTypes.FirstOrDefault();
        }
    }
}
