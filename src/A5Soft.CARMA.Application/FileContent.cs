using System;
using System.IO;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A container class for file data and metadata.
    /// </summary>
    [Serializable]
    public class FileContent : IDisposable
    {

        public FileContent(Stream content, string contentType, string fileName)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }


        /// <summary>
        /// Gets a (binary) file content.
        /// </summary>
        public Stream Content { get; }

        /// <summary>
        /// Gets a file content MIME type as defined by IANA.
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Gets a name of the file.
        /// </summary>
        public string FileName { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            Content.Dispose();
        }

    }
}
