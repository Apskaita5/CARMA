using System;
using System.IO;

namespace A5Soft.CARMA.Application.DataPortal
{
    /// <summary>
    /// A file of an outgoing call, as the client data portal hands it to a proxy.
    /// </summary>
    /// <remarks>The content stream enforces the data portal limits as it is read, and the proxy must
    /// not dispose it: the caller of the use case owns the underlying file.</remarks>
    public sealed class DataPortalFile
    {
        internal DataPortalFile(string fileName, string contentType, Stream content, long? length)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Length = length;
        }


        /// <summary>Gets a name of the file, as the caller gave it.</summary>
        public string FileName { get; }

        /// <summary>Gets a media (MIME) type of the file, as the caller gave it.</summary>
        public string ContentType { get; }

        /// <summary>
        /// Gets the content to send: forward-only, read once. Reading past a data portal limit throws.
        /// </summary>
        public Stream Content { get; }

        /// <summary>
        /// Gets the length of the file in bytes, when it is known in advance (a seekable source);
        /// null otherwise.
        /// </summary>
        public long? Length { get; }
    }
}
