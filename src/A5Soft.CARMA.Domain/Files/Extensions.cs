using System;

namespace A5Soft.CARMA.Domain.Files
{
    /// <summary>
    /// Extension methods for handling files.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets a type of the file for the file extension specified.
        /// </summary>
        /// <param name="fileExtension">a file extension to get a file type for</param>
        /// <returns>a type of the file</returns>
        public static FileType GetFileType(this string fileExtension)
        {
            if (fileExtension.IsNullOrWhiteSpace()) return FileType.Unknown;

            var ext = fileExtension.Trim().TrimStart('.');
            foreach (var value in Enum.GetValues(typeof(FileType)))
            {
                if (value.ToString().Equals(ext, StringComparison.OrdinalIgnoreCase)) return (FileType)value;
            }
            return FileType.Unknown;
        }

        /// <summary>
        /// Gets a file media (MIME) type.
        /// </summary>
        /// <param name="fileType">a type of the file to get the media (MIME) type for</param>
        /// <returns>file media (MIME) type</returns>
        public static string GetFileMediaType(this FileType fileType)
        {
            switch (fileType)
            {
                case FileType.Adoc:
                    return FileContentType.Adoc;
                case FileType.Csv:
                    return FileContentType.Csv;
                case FileType.Doc:
                    return FileContentType.MsWord;
                case FileType.Docx:
                    return FileContentType.MsWord;
                case FileType.GZip:
                    return FileContentType.GZip;
                case FileType.Html:
                    return FileContentType.Html;
                case FileType.Json:
                    return FileContentType.Json;
                case FileType.Pdf:
                    return FileContentType.Pdf;
                case FileType.Png:
                    return FileContentType.Png;
                case FileType.Rtf:
                    return FileContentType.Rtf;
                case FileType.Tiff:
                    return FileContentType.Tiff;
                case FileType.Txt:
                    return FileContentType.PlainText;
                case FileType.Unknown:
                    return FileContentType.Binary;
                case FileType.Xls:
                    return FileContentType.MsExcel;
                case FileType.Xlsx:
                    return FileContentType.MsExcel;
                case FileType.Zip:
                    return FileContentType.Zip;
                case FileType.Xml:
                    return FileContentType.Xml;
                default:
                    throw new NotImplementedException($"File type {fileType.ToString()} is not implemented in method {nameof(GetFileMediaType)}.");
            }
        }

        /// <summary>
        /// Gets an extension for the file of the specified type.
        /// </summary>
        /// <param name="fileType">a type of the file to get an extension for</param>
        /// <returns>an extension for the file (prefixed with .)</returns>
        public static string GetFileExtension(this FileType fileType)
        {
            return $".{fileType.ToString().ToLower()}";
        }
    }
}
