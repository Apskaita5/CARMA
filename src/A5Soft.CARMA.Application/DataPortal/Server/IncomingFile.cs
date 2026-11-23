using System;
using System.IO;

namespace A5Soft.CARMA.Application.DataPortal.Server
{
    /// <summary>
    /// A file of an incoming call, as a server endpoint hands it to the server data portal.
    /// </summary>
    /// <remarks>Every value is untrusted: the server data portal sanitizes the name and the content
    /// type, and enforces the limits as it reads the content.</remarks>
    public sealed class IncomingFile
    {
        /// <summary>
        /// Creates a new incoming file.
        /// </summary>
        /// <param name="fileName">a name of the file as received; null if none</param>
        /// <param name="contentType">a media type of the file as received; null if none</param>
        /// <param name="content">the content as it arrives; the endpoint keeps owning it</param>
        public IncomingFile(string fileName, string contentType, Stream content)
        {
            FileName = fileName;
            ContentType = contentType;
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }


        /// <summary>Gets a name of the file as received; null if none.</summary>
        public string FileName { get; }

        /// <summary>Gets a media type of the file as received; null if none.</summary>
        public string ContentType { get; }

        /// <summary>
        /// Gets the content as it arrives. It must be read to its end before the next file is
        /// requested. A transport failure or malformed data surfaces as a
        /// <see cref="RemoteCallRejectedException"/>.
        /// </summary>
        public Stream Content { get; }
    }
}
