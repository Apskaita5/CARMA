using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal.Internal
{
    /// <summary>
    /// The body of a downloaded file as the client receives it, which releases the response
    /// (and with it the connection) when the caller disposes it.
    /// </summary>
    internal sealed class OwningReadStream : Stream
    {
        private readonly Stream _inner;
        private readonly long? _length;
        private IDisposable _owner;
        private long _position;


        public OwningReadStream(Stream inner, long? length, IDisposable owner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _length = length;
            _owner = owner;
        }


        public override bool CanRead => null != _owner;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        /// <summary>Gets the length of the file, when the server sent it.</summary>
        public override long Length => _length ?? throw new NotSupportedException();

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }


        public override int Read(byte[] buffer, int offset, int count)
            => Count(_inner.Read(buffer, offset, count));

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
            => Count(await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false));

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
            CancellationToken cancellationToken = default)
            => Count(await _inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false));

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Interlocked.Exchange(ref _owner, null)?.Dispose();
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }


        private int Count(int read)
        {
            _position += read;
            return read;
        }
    }
}
