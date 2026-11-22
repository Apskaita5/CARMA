using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using A5Soft.CARMA.Domain.Files;

namespace A5Soft.CARMA.Application.DataPortal.Internal
{
    /// <summary>
    /// The per-call state of the data portal converters, carried in the serializer's streaming context:
    /// which subclass is present decides what the <see cref="FileContentConverter"/> does with a file, and
    /// <see cref="Types"/> is what the <see cref="TrustedWireTypeConverter"/> resolves type names through
    /// (<see cref="RemoteUseCaseRegistry.TrustedWireTypes"/>).
    /// </summary>
    internal abstract class FileTransfer
    {
        protected FileTransfer(IReadOnlyDictionary<string, Type> types)
        {
            Types = types ?? throw new ArgumentNullException(nameof(types));
        }

        /// <summary>Gets the types that may be named on the wire in this call.</summary>
        public IReadOnlyDictionary<string, Type> Types { get; }
    }

    /// <summary>
    /// Client, request: files are detached from the graph into a list, and replaced by references.
    /// The same instance referenced twice is sent once.
    /// </summary>
    internal sealed class OutgoingFiles : FileTransfer
    {
        private readonly Dictionary<FileContent, int> _indexes
            = new Dictionary<FileContent, int>(ReferenceComparer.Instance);
        private readonly List<FileContent> _files = new List<FileContent>();


        public OutgoingFiles(IReadOnlyDictionary<string, Type> types) : base(types) { }


        /// <summary>Gets the files detached, in the order of their references.</summary>
        public IReadOnlyList<FileContent> Files => _files;


        /// <summary>Registers a file and gets its reference.</summary>
        public int Add(FileContent file)
        {
            if (_indexes.TryGetValue(file, out var index)) return index;

            index = _files.Count;
            _files.Add(file);
            _indexes.Add(file, index);

            return index;
        }


        private sealed class ReferenceComparer : IEqualityComparer<FileContent>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();
            public bool Equals(FileContent x, FileContent y) => ReferenceEquals(x, y);
            public int GetHashCode(FileContent obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }

    /// <summary>
    /// Server, request: references are resolved to the files received, which are already spooled.
    /// A reference may be used more than once; every file must be referenced at least once.
    /// </summary>
    internal sealed class IncomingFiles : FileTransfer
    {
        private readonly IReadOnlyList<FileContent> _files;
        private readonly bool[] _claimed;


        public IncomingFiles(IReadOnlyList<FileContent> files, IReadOnlyDictionary<string, Type> types) : base(types)
        {
            _files = files ?? throw new ArgumentNullException(nameof(files));
            _claimed = new bool[files.Count];
        }


        /// <summary>Gets a value indicating whether every file has been referenced.</summary>
        public bool AllClaimed => Array.TrueForAll(_claimed, c => c);


        /// <summary>Gets the file with the reference given.</summary>
        /// <exception cref="RemoteCallRejectedException">no file has the reference</exception>
        public FileContent Claim(int index)
        {
            if (index < 0 || index >= _files.Count) throw new RemoteCallRejectedException(
                RemoteErrorKind.BadRequest, $"The payload references file {index}, which was not sent.");

            _claimed[index] = true;
            return _files[index];
        }
    }

    /// <summary>
    /// Server, response: files travel upstream only, so a file nested in a result is written as null.
    /// </summary>
    internal sealed class ResponseFiles : FileTransfer
    {
        public ResponseFiles(IReadOnlyDictionary<string, Type> types) : base(types) { }
    }

    /// <summary>
    /// Client, response: a file reference in a result is never valid.
    /// </summary>
    internal sealed class NoFiles : FileTransfer
    {
        public NoFiles(IReadOnlyDictionary<string, Type> types) : base(types) { }
    }
}
