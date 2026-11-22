using System;
using System.IO;
using System.Runtime.InteropServices;

namespace A5Soft.CARMA.Application.DataPortal.Internal
{
    /// <summary>Extension methods for efficient memory handling.</summary>
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
    /// later carry a pooled buffer (IMemoryOwner<byte>) without a change of contract.</para>
    /// <para>The memory is valid only while its owner lives: the client data portal until the
    /// request has been sent, a DataPortalResponse until it is disposed.</para>
    /// </remarks>
    internal static class PayloadBytes
    {
        // Gets the bytes written to a stream so far, over its own buffer: no copy.
        public static ReadOnlyMemory<byte> WrittenBytes(this MemoryStream stream)
            => new ReadOnlyMemory<byte>(stream.GetBuffer(), 0, checked((int)stream.Length));

        // Opens a read-only stream over the bytes, for Json.NET, which reads streams only.
        // No copy when the memory is backed by an array, which it always is in the data portal.
        public static MemoryStream OpenRead(this ReadOnlyMemory<byte> bytes)
            => MemoryMarshal.TryGetArray(bytes, out var segment)
                ? new MemoryStream(segment.Array, segment.Offset, segment.Count, false)
                // memory over native or pooled storage (a custom MemoryManager) has no array: copy it once
                : new MemoryStream(bytes.ToArray(), false);
    }
}
