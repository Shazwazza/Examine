using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Examine;
using iTextSharp.text.pdf;

namespace UmbracoExamine.Contrib
{
    /// <summary>
    /// An Umbraco Lucene.Net indexer which will index the text content of a file
    /// </summary>
    public sealed class FileIndexer : LuceneExamineIndexer
    {
        /// <summary>
        /// Gets or sets the supported extensions for files
        /// </summary>
        /// <value>The supported extensions.</value>
        public IEnumerable<string> SupportedExtensions { get; protected set; }
        /// <summary>
        /// Gets or sets the umbraco property alias (defaults to umbracoFile)
        /// </summary>
        /// <value>The umbraco file property.</value>
        public string UmbracoFileProperty { get; protected set; }
        /// <summary>
        /// Gets the name of the Lucene.Net field which the content is inserted into
        /// </summary>
        /// <value>The name of the text content field.</value>
        public string TextContentFieldName
        {
            get
            {
                return "FileTextContent";
            }
        }

        /// <summary>
        /// Set up all properties for the indexer based on configuration information specified. This will ensure that
        /// all of the folders required by the indexer are created and exist. This will also create an instruction
        /// file declaring the computer name that is part taking in the indexing. This file will then be used to
        /// determine the master indexer machine in a load balanced environment (if one exists).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            if (string.IsNullOrEmpty(config["extensions"]))
                SupportedExtensions = new[] { ".pdf" };
            else
                SupportedExtensions = config["extensions"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            //checks if a custom field alias is specified
            if (string.IsNullOrEmpty(config["umbracoFileProperty"]))
                UmbracoFileProperty = "umbracoFile";
            else
                UmbracoFileProperty = config["umbracoFileProperty"];
        }

        /// <summary>
        /// Re-indexes all data for the index type specified
        /// </summary>
        /// <param name="type"></param>
        public override void IndexAll(string type)
        {
            //ignore the content index types
            if (type == "Content")
                return;

            base.IndexAll(type);
        }

        /// <summary>
        /// Collects all of the data that needs to be indexed as defined in the index set.
        /// </summary>
        /// <param name="node">Media item XML being indexed</param>
        /// <param name="type">Type of index (should only ever be media)</param>
        /// <returns>Fields containing the data for the index</returns>
        protected override Dictionary<string, string> GetDataToIndex(XElement node, string type)
        {
            var fields = base.GetDataToIndex(node, type);

            //find the field which contains the file
            var filePath = node.Elements().FirstOrDefault(x =>
            {
                if (x.Attribute("alias") != null)
                    return (string)x.Attribute("alias") == this.UmbracoFileProperty;
                else
                    return x.Name == this.UmbracoFileProperty;
            });
            //make sure the file exists
            if (filePath != default(XElement) && !string.IsNullOrEmpty((string)filePath))
            {
                //get the file path from the data service
                var fullPath = this.DataService.MapPath((string)filePath);
                var fi = new FileInfo(fullPath);
                if (fi.Exists)
                {
                    //check if the extension of the file matches one we're supporting
                    foreach (var ext in SupportedExtensions)
                    {
                        if (fi.Extension.ToUpper() == ext.ToUpper())
                        {
                            switch (ext.ToUpper())
                            {
                                    //extract the content from the PDF
                                case ".PDF":
                                    PdfTextExtractor pdfTextExtractor = new PdfTextExtractor();
                                    fields.Add(this.TextContentFieldName, pdfTextExtractor.ExtractText(fi.FullName));
                                    break;

                                default:
                                    //log that we couldn't index the file found
                                    DataService.LogService.AddInfoLog((int)node.Attribute("id"), "UmbracoExamine.FileIndexer: Extension '" + ext + "' is not supported at this time");
                                    break;
                            }
                        }
                    }
                }
                else
                {
                  DataService.LogService.AddInfoLog((int)node.Attribute("id"), "UmbracoExamine.FileIndexer: No file found at path " + filePath);
                }
            }
            else
            {
                DataService.LogService.AddInfoLog((int)node.Attribute("id"), "UmbracoExamine.FileIndexer: No data found at data alias \"umbracoFile\"");
            }

            return fields;
        }

        /// <summary>
        /// From: http://stackoverflow.com/questions/83152/reading-pdf-documents-in-net/84410#84410
        /// </summary>
        class PdfTextExtractor
        {
            /// BT = Beginning of a text object operator 
            /// ET = End of a text object operator
            /// Td move to the start of next line
            ///  5 Ts = superscript
            /// -5 Ts = subscript

            #region Fields

            #region _numberOfCharsToKeep
            /// <summary>
            /// The number of characters to keep, when extracting text.
            /// </summary>
            private static int _numberOfCharsToKeep = 15;
            #endregion

            #endregion

            #region ExtractText
            /// <summary>
            /// Extracts a text from a PDF file.
            /// </summary>
            /// <param name="inFileName">the full path to the pdf file.</param>
            /// <param name="outFileName">the output file name.</param>
            /// <returns>the extracted text</returns>
            public string ExtractText(string inFileName)
            {
                StringWriter writer = null;
                try
                {
                    // Create a reader for the given PDF file
                    PdfReader reader = new PdfReader(inFileName);
                    writer = new StringWriter();

                    Console.Write("Processing: ");

                    int totalLen = 68;
                    float charUnit = ((float)totalLen) / (float)reader.NumberOfPages;
                    int totalWritten = 0;
                    float curUnit = 0;

                    for (int page = 1; page <= reader.NumberOfPages; page++)
                    {
                        writer.Write(ExtractTextFromPDFBytes(reader.GetPageContent(page)) + " ");

                        // Write the progress.
                        if (charUnit >= 1.0f)
                        {
                            for (int i = 0; i < (int)charUnit; i++)
                            {
                                Console.Write("#");
                                totalWritten++;
                            }
                        }
                        else
                        {
                            curUnit += charUnit;
                            if (curUnit >= 1.0f)
                            {
                                for (int i = 0; i < (int)curUnit; i++)
                                {
                                    Console.Write("#");
                                    totalWritten++;
                                }
                                curUnit = 0;
                            }

                        }
                    }

                    if (totalWritten < totalLen)
                    {
                        for (int i = 0; i < (totalLen - totalWritten); i++)
                        {
                            Console.Write("#");
                        }
                    }
                    return writer.ToString();
                }
                catch
                {
                    return string.Empty;
                }
                finally
                {
                    if (writer != null) writer.Close();
                }
            }
            #endregion

            #region ExtractTextFromPDFBytes
            /// <summary>
            /// This method processes an uncompressed Adobe (text) object 
            /// and extracts text.
            /// </summary>
            /// <param name="input">uncompressed</param>
            /// <returns></returns>
            public string ExtractTextFromPDFBytes(byte[] input)
            {
                if (input == null || input.Length == 0) return "";

                try
                {
                    string resultString = "";

                    // Flag showing if we are we currently inside a text object
                    bool inTextObject = false;

                    // Flag showing if the next character is literal 
                    // e.g. '\\' to get a '\' character or '\(' to get '('
                    bool nextLiteral = false;

                    // () Bracket nesting level. Text appears inside ()
                    int bracketDepth = 0;

                    // Keep previous chars to get extract numbers etc.:
                    char[] previousCharacters = new char[_numberOfCharsToKeep];
                    for (int j = 0; j < _numberOfCharsToKeep; j++) previousCharacters[j] = ' ';


                    for (int i = 0; i < input.Length; i++)
                    {
                        char c = (char)input[i];
                        if (input[i] == 213)
                            c = "'".ToCharArray()[0];

                        if (inTextObject)
                        {
                            // Position the text
                            if (bracketDepth == 0)
                            {
                                if (CheckToken(new string[] { "TD", "Td" }, previousCharacters))
                                {
                                    resultString += "\n\r";
                                }
                                else
                                {
                                    if (CheckToken(new string[] { "'", "T*", "\"" }, previousCharacters))
                                    {
                                        resultString += "\n";
                                    }
                                    else
                                    {
                                        if (CheckToken(new string[] { "Tj" }, previousCharacters))
                                        {
                                            resultString += " ";
                                        }
                                    }
                                }
                            }

                            // End of a text object, also go to a new line.
                            if (bracketDepth == 0 &&
                                CheckToken(new string[] { "ET" }, previousCharacters))
                            {

                                inTextObject = false;
                                resultString += " ";
                            }
                            else
                            {
                                // Start outputting text
                                if ((c == '(') && (bracketDepth == 0) && (!nextLiteral))
                                {
                                    bracketDepth = 1;
                                }
                                else
                                {
                                    // Stop outputting text
                                    if ((c == ')') && (bracketDepth == 1) && (!nextLiteral))
                                    {
                                        bracketDepth = 0;
                                    }
                                    else
                                    {
                                        // Just a normal text character:
                                        if (bracketDepth == 1)
                                        {
                                            // Only print out next character no matter what. 
                                            // Do not interpret.
                                            if (c == '\\' && !nextLiteral)
                                            {
                                                resultString += c.ToString();
                                                nextLiteral = true;
                                            }
                                            else
                                            {
                                                if (((c >= ' ') && (c <= '~')) ||
                                                    ((c >= 128) && (c < 255)))
                                                {
                                                    resultString += c.ToString();
                                                }

                                                nextLiteral = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Store the recent characters for 
                        // when we have to go back for a checking
                        for (int j = 0; j < _numberOfCharsToKeep - 1; j++)
                        {
                            previousCharacters[j] = previousCharacters[j + 1];
                        }
                        previousCharacters[_numberOfCharsToKeep - 1] = c;

                        // Start of a text object
                        if (!inTextObject && CheckToken(new string[] { "BT" }, previousCharacters))
                        {
                            inTextObject = true;
                        }
                    }

                    return CleanupContent(resultString);
                }
                catch
                {
                    return "";
                }
            }

            private string CleanupContent(string text)
            {
                string[] patterns = { @"\\\(", @"\\\)", @"\\226", @"\\222", @"\\223", @"\\224", @"\\340", @"\\342", @"\\344", @"\\300", @"\\302", @"\\304", @"\\351", @"\\350", @"\\352", @"\\353", @"\\311", @"\\310", @"\\312", @"\\313", @"\\362", @"\\364", @"\\366", @"\\322", @"\\324", @"\\326", @"\\354", @"\\356", @"\\357", @"\\314", @"\\316", @"\\317", @"\\347", @"\\307", @"\\371", @"\\373", @"\\374", @"\\331", @"\\333", @"\\334", @"\\256", @"\\231", @"\\253", @"\\273", @"\\251", @"\\221" };
                string[] replace = { "(", ")", "-", "'", "\"", "\"", "à", "â", "ä", "À", "Â", "Ä", "é", "è", "ê", "ë", "É", "È", "Ê", "Ë", "ò", "ô", "ö", "Ò", "Ô", "Ö", "ì", "î", "ï", "Ì", "Î", "Ï", "ç", "Ç", "ù", "û", "ü", "Ù", "Û", "Ü", "®", "™", "«", "»", "©", "'" };

                for (int i = 0; i < patterns.Length; i++)
                {
                    string regExPattern = patterns[i];
                    Regex regex = new Regex(regExPattern, RegexOptions.IgnoreCase);
                    text = regex.Replace(text, replace[i]);
                }

                return text;
            }

            #endregion

            #region CheckToken
            /// <summary>
            /// Check if a certain 2 character token just came along (e.g. BT)
            /// </summary>
            /// <param name="tokens">the searched token</param>
            /// <param name="recent">the recent character array</param>
            /// <returns></returns>
            private bool CheckToken(string[] tokens, char[] recent)
            {
                foreach (string token in tokens)
                {
                    if ((recent[_numberOfCharsToKeep - 3] == token[0]) &&
                        (recent[_numberOfCharsToKeep - 2] == token[1]) &&
                        ((recent[_numberOfCharsToKeep - 1] == ' ') ||
                        (recent[_numberOfCharsToKeep - 1] == 0x0d) ||
                        (recent[_numberOfCharsToKeep - 1] == 0x0a)) &&
                        ((recent[_numberOfCharsToKeep - 4] == ' ') ||
                        (recent[_numberOfCharsToKeep - 4] == 0x0d) ||
                        (recent[_numberOfCharsToKeep - 4] == 0x0a))
                        )
                    {
                        return true;
                    }
                }
                return false;
            }
            #endregion
        }
    }
}
