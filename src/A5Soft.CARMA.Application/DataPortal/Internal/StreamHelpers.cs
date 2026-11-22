using System;
using System.IO;
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
    }
}
