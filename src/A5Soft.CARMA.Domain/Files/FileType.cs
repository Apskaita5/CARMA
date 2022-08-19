namespace A5Soft.CARMA.Domain.Files
{
    /// <summary>
    /// A types of files handled by the app.
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// Unkown file type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// xml file
        /// </summary>
        Xml = 1,

        /// <summary>
        /// pdf file
        /// </summary>
        Pdf = 2,

        /// <summary>
        /// doc file (old word)
        /// </summary>
        Doc = 3,

        /// <summary>
        /// docx file (current word version)
        /// </summary>
        Docx = 4,

        /// <summary>
        /// xls file (old excel)
        /// </summary>
        Xls = 5,

        /// <summary>
        /// xlss file (current excel version)
        /// </summary>
        Xlsx = 6,

        /// <summary>
        /// json file
        /// </summary>
        Json = 7,

        /// <summary>
        /// png image file
        /// </summary>
        Png = 8,

        /// <summary>
        /// tiff image file
        /// </summary>
        Tiff = 9,

        /// <summary>
        /// html file
        /// </summary>
        Html = 10,

        /// <summary>
        /// csv (comma separated values) file
        /// </summary>
        Csv = 11,

        /// <summary>
        /// rtf (rich text) file
        /// </summary>
        Rtf = 12,

        /// <summary>
        /// plain text file
        /// </summary>
        Txt = 13,

        /// <summary>
        /// zip file
        /// </summary>
        Zip = 14,

        /// <summary>
        /// gzip file
        /// </summary>
        GZip = 15,

        /// <summary>
        /// adoc file (signed document)
        /// </summary>
        Adoc = 16
    }
}
