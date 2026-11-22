using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal.Internal
{
    /// <summary>
    /// The files of an outgoing call, counted as a proxy reads them: enforces the per-file and total
    /// limits, so that a file of unknown length over a limit is stopped before its excess is sent,
    /// whatever the transport.
    /// </summary>
    internal sealed class SendTracker
    {
        private readonly long _maxFileLength;
        private readonly long _maxUploadLength;
        private long _total;
        private int _anyRead;


        public SendTracker(long maxFileLength, long maxUploadLength)
        {
            _maxFileLength = maxFileLength;
            _maxUploadLength = maxUploadLength;
        }


        /// <summary>Gets a value indicating whether any byte of any file has been read.</summary>
        public bool AnyRead => Volatile.Read(ref _anyRead) != 0;

        /// <summary>Gets the limit violation found, if any.</summary>
        public string Violation { get; private set; }


        /// <summary>Wraps the content of a file to send.</summary>
        public Stream Track(Stream content, string fileName) => new TrackedStream(this, content, fileName);


        private void Count(TrackedStream file, int read)
        {
            if (read < 1) return;

            Interlocked.Exchange(ref _anyRead, 1);
            file.BytesRead += read;
            var total = Interlocked.Add(ref _total, read);

            if (file.BytesRead > _maxFileLength)
                Violation = $"file {file.FileName} is longer than the {_maxFileLength} bytes allowed";
            else if (total > _maxUploadLength)
                Violation = $"the files are longer than the {_maxUploadLength} bytes allowed";

            if (null != Violation) throw new InvalidOperationException(
                $"The call exceeds the data portal limits: {Violation}.");
        }


        /// <summary>
        /// A forward-only view of the caller's stream that counts what is read. Disposing it does not
        /// dispose the caller's stream.
        /// </summary>
        private sealed class TrackedStream : Stream
        {
            private readonly SendTracker _tracker;
            private readonly Stream _inner;

            public TrackedStream(SendTracker tracker, Stream inner, string fileName)
            {
                _tracker = tracker;
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
                FileName = fileName;
            }

            public string FileName { get; }
            public long BytesRead;

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => BytesRead; set => throw new NotSupportedException(); }

            public override int Read(byte[] buffer, int offset, int count)
            {
                var read = _inner.Read(buffer, offset, count);
                _tracker.Count(this, read);
                return read;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                var read = await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
                _tracker.Count(this, read);
                return read;
            }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                var read = await _inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                _tracker.Count(this, read);
                return read;
            }

            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
