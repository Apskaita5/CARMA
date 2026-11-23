using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.DataPortal.Internal
{
    /// <summary>
    /// Asynchronous reads of bounded JSON segments. JSON is read into bytes, never into strings,
    /// and parsed from memory, because Json.NET parses synchronously and a host such as
    /// ASP.NET Core forbids synchronous reads of a request body.
    /// </summary>
    internal static class StreamHelpers
    {
        /// <summary>
        /// Reads exactly the number of bytes given.
        /// </summary>
        /// <returns>the bytes read; false if the stream ended first</returns>
        public static async Task<bool> ReadExactlyAsync(this Stream stream, byte[] buffer, int count,
            CancellationToken cancellationToken)
        {
            var offset = 0;
            while (offset < count)
            {
                var read = await stream.ReadAsync(buffer, offset, count - offset, cancellationToken)
                    .ConfigureAwait(false);
                if (read < 1) return false;
                offset += read;
            }

            return true;
        }

        /// <summary>
        /// Reads a stream to its end into memory.
        /// </summary>
        /// <param name="stream">a stream to read</param>
        /// <param name="maxLength">a max number of bytes to accept</param>
        /// <param name="cancellationToken">a token to cancel the read</param>
        /// <returns>a memory stream positioned at its start; null if the stream is longer than allowed</returns>
        public static async Task<MemoryStream> ReadToEndAsync(this Stream stream, long maxLength,
            CancellationToken cancellationToken)
        {
            var result = new MemoryStream();
            var buffer = new byte[16 * 1024];
            int read;

            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                .ConfigureAwait(false)) > 0)
            {
                if (result.Length + read > maxLength) return null;
                result.Write(buffer, 0, read);
            }

            result.Position = 0;
            return result;
        }

        /// <summary>
        /// Checks whether a stream has any more bytes, consuming at most one.
        /// </summary>
        public static async Task<bool> HasMoreAsync(this Stream stream, CancellationToken cancellationToken)
        {
            var buffer = new byte[1];
            return await stream.ReadAsync(buffer, 0, 1, cancellationToken).ConfigureAwait(false) > 0;
        }

        /// <summary>
        /// Gets the bytes written to a stream so far, over its own buffer: no copy.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>the bytes written to a stream so far</returns>
        public static ReadOnlyMemory<byte> WrittenBytes(this MemoryStream stream)
            => new ReadOnlyMemory<byte>(stream.GetBuffer(), 0, checked((int)stream.Length));

        /// <summary>
        /// Opens a read-only stream over the bytes, for Json.NET, which reads streams only.
        /// No copy when the memory is backed by an array, which it always is in the data portal.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>a read-only stream over the bytes</returns>
        /// <remarks>
        /// <para>Why ReadOnlyMemory rather than byte[]: the JSON is always produced into a MemoryStream,
        /// as its length is not known in advance. A byte[] of the exact length would take ToArray():
        /// a second allocation and a full copy for every payload and every result, up to MaxPayloadLength
        /// or MaxResponseLength. Anything over 85,000 bytes lands on the large object heap, which only a
        /// full collection reclaims, and peak memory doubles while the copy is made. Wrapping the
        /// stream's own buffer (GetBuffer(), longer than the content) avoids all of it.</para>
        /// <para>Why not ArraySegment, which does the same: a segment exposes a writable array, has a
        /// null state (default), and every reader must remember to respect its offset and count.
        /// ReadOnlyMemory is read-only, has no null state (default is empty), slices safely, and could
        /// later carry a pooled buffer (<c>IMemoryOwner&lt;byte&gt;</c>) without a change of contract.</para>
        /// <para>The memory is valid only while its owner lives: the client data portal until the
        /// request has been sent, a DataPortalResponse until it is disposed.</para>
        /// </remarks>
        public static MemoryStream OpenRead(this ReadOnlyMemory<byte> bytes)
            => MemoryMarshal.TryGetArray(bytes, out var segment)
                ? new MemoryStream(segment.Array, segment.Offset, segment.Count, false)
                // memory over native or pooled storage (a custom MemoryManager) has no array: copy it once
                : new MemoryStream(bytes.ToArray(), false);
    }
}
