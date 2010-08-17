using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Examine;
using iTextSharp.text.pdf;
using System.Text;


namespace UmbracoExamine
{
    /// <summary>
    /// An Umbraco Lucene.Net indexer which will index the text content of a file
    /// </summary>
    public class PDFIndexer : BaseUmbracoIndexer
    {

        /// <summary>
        /// Gets or sets the supported extensions for files, currently the system will only
        /// process PDF files.
        /// </summary>
        /// <value>The supported extensions.</value>
        public IEnumerable<string> SupportedExtensions { get; private set; }

        /// <summary>
        /// Gets or sets the umbraco property alias (defaults to umbracoFile)
        /// </summary>
        /// <value>The umbraco file property.</value>
        public string UmbracoFileProperty { get; private set; }

        /// <summary>
        /// Gets the name of the Lucene.Net field which the content is inserted into
        /// </summary>
        /// <value>The name of the text content field.</value>
        public const string TextContentFieldName = "FileTextContent";

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
        /// The PDF Indexer only supports indexing PDFs in the media application. 
        /// This will rebuild the index for only the media items.
        /// </summary>
        protected override void PerformIndexRebuild()
        {
            IndexAll(IndexTypes.Media);
        }

        /// <summary>
        /// Re-indexes media content, if a type other than IndexTypes.Media is
        /// passed in, it will be ignored.
        /// </summary>
        /// <param name="type"></param>
        protected override void PerformIndexAll(string type)
        {

            //ignore the content index types
            if (type == IndexTypes.Media)
            {
                base.PerformIndexAll(type);
            }

        }

        /// <summary>
        /// Provides the means to extract the text to be indexed from the file specified
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected virtual string ExtractTextFromFile(FileInfo file)
        {
            if (!SupportedExtensions.Select(x => x.ToUpper()).Contains(file.Extension.ToUpper()))
            {
                throw new NotSupportedException("The file with the extension specified is not supported");
            }

            var pdf = new PDFParser();

            var txt = pdf.ParsePdfText(file.FullName);
            return txt;

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
                    try
                    {
                        fields.Add(TextContentFieldName, ExtractTextFromFile(fi));
                    }
                    catch (NotSupportedException)
                    {
                        //log that we couldn't index the file found
                        DataService.LogService.AddErrorLog((int)node.Attribute("id"), "UmbracoExamine.FileIndexer: Extension '" + fi.Extension + "' is not supported at this time");
                    }
                }
                else
                {
                    DataService.LogService.AddInfoLog((int)node.Attribute("id"), "UmbracoExamine.FileIndexer: No file found at path " + filePath);
                }
            }

            return fields;
        }

        /// <summary>
        /// Parses a PDF file and extracts the text from it.
        /// </summary>
        public class PDFParser
        {

            public string ParsePdfText(string sourcePDF)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                PdfReader reader = new PdfReader(sourcePDF);
                byte[] pageBytes = null;
                PRTokeniser token = null;
                PRTokeniser.TokType tknType;
                string tknValue = String.Empty;
              
                for (var i = 1; (i <= reader.NumberOfPages); i++)
                {
                    pageBytes = reader.GetPageContent(i);
                    if (pageBytes != null)
                    {
                        token = new PRTokeniser(pageBytes);
                        while (token.NextToken())
                        {
                            tknType = token.TokenType;
                            tknValue = token.StringValue;
                            if ((tknType == PRTokeniser.TokType.STRING))
                            {

                                //this check seems to work to exclude non-standard tokens/strings.
                                //not sure if it's something to do with octal or something else i read, but oh well
                                if (token.StringValue[0] >= 0 && token.StringValue[0] <= 9)
                                {
                                    //var x = -1;
                                    //while (token.StringValue.Length - x > 3)
                                    //{
                                        //var first = ((int)token.StringValue[++x]).ToString();
                                        //var second = ((int)token.StringValue[++x]).ToString();
                                        //var third = ((int)token.StringValue[++x]).ToString();
                                        //var octalCode = first +
                                        //    second +
                                        //    third;
                                        //var c = (char)Convert.ToInt32(octalCode, 8);
                                        //sb.Append(c);
                                    //}
                                    
                                }
                                else
                                {
                                    sb.Append(token.StringValue);
                                }
                                

                            }
                            // I need to add these additional tests to properly add whitespace to the output string
                            //else if (((tknType == PRTokeniser.TokType.NUMBER) && (tknValue == "-600")))
                            //{
                            //    sb.Append(" ");
                            //}
                            //else if (((tknType == PRTokeniser.TokType.OTHER) && (tknValue == "TJ")))
                            //{
                            //    sb.Append(" ");
                            //}
                        }
                    }
                }
                
                return sb.ToString();
            }

            public static String ConvertToOctal(String input)
            {
                String resultString = "";
                char c;
                for (int i = 0; i < input.Length; i++)
                {
                    c = (char)input[i];
                    if (c > 500)
                    {
                        resultString += @"\" + Convert.ToString(input[i], 8);
                    }
                    else
                    {
                        resultString += c;
                    }
                }
                return resultString;
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

        }

    }
}
