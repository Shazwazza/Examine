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
using Lucene.Net.Analysis;
using UmbracoExamine.DataServices;


namespace UmbracoExamine.PDF
{
    /// <summary>
    /// An Umbraco Lucene.Net indexer which will index the text content of a file
    /// </summary>
    public class PDFIndexer : BaseUmbracoIndexer
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PDFIndexer()
        {
            SupportedExtensions = new[] { ".pdf" };
            UmbracoFileProperty = "umbracoFile";
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        public PDFIndexer(DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer)
            : base(
                new IndexCriteria(Enumerable.Empty<IIndexField>(), Enumerable.Empty<IIndexField>(), Enumerable.Empty<string>(), Enumerable.Empty<string>(), null), 
                indexPath, dataService, analyzer)
        {
            SupportedExtensions = new[] { ".pdf" };
            UmbracoFileProperty = "umbracoFile";
        }

        #endregion


        #region Properties
        /// <summary>
        /// Gets or sets the supported extensions for files, currently the system will only
        /// process PDF files.
        /// </summary>
        /// <value>The supported extensions.</value>
        public IEnumerable<string> SupportedExtensions { get; set; }

        /// <summary>
        /// Gets or sets the umbraco property alias (defaults to umbracoFile)
        /// </summary>
        /// <value>The umbraco file property.</value>
        public string UmbracoFileProperty { get; set; }

        /// <summary>
        /// Gets the name of the Lucene.Net field which the content is inserted into
        /// </summary>
        /// <value>The name of the text content field.</value>
        public const string TextContentFieldName = "FileTextContent";

        protected override IEnumerable<string> SupportedTypes
        {
            get
            {
                return new string[] { IndexTypes.Media };
            }
        }

        #endregion

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

            if (!string.IsNullOrEmpty(config["extensions"]))
                SupportedExtensions = config["extensions"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            //checks if a custom field alias is specified
            if (!string.IsNullOrEmpty(config["umbracoFileProperty"]))                
                UmbracoFileProperty = config["umbracoFileProperty"];
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
        
        #region Internal PDFParser Class

        /// <summary>
        /// Parses a PDF file and extracts the text from it.
        /// </summary>
        internal class PDFParser
        {

            static PDFParser()
            {
                lock (m_Locker)
                {
                    m_UnsupportedRange = new List<int>();
                    m_UnsupportedRange.AddRange(Enumerable.Range(0x0000, 0x001F));
                    m_UnsupportedRange.Add(0x1F);

                }
            }

            private static readonly object m_Locker = new object();

            /// <summary>
            /// Stores the unsupported range of character
            /// </summary>
            /// <remarks>
            /// used as a reference:
            /// http://www.tamasoft.co.jp/en/general-info/unicode.html
            /// http://en.wikipedia.org/wiki/Summary_of_Unicode_character_assignments
            /// http://en.wikipedia.org/wiki/Unicode
            /// http://en.wikipedia.org/wiki/Basic_Multilingual_Plane
            /// </remarks>
            private static List<int> m_UnsupportedRange;

            /// <summary>
            /// Return only the valid string contents of the PDF
            /// </summary>
            /// <param name="sourcePDF"></param>
            /// <returns></returns>
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
                                foreach (var s in tknValue)
                                {
                                    //strip out unsupported characters, based on unicode tables.
                                    if (!m_UnsupportedRange.Contains(s))
                                    {
                                        sb.Append(s);
                                    }
                                }

                            }
                        }
                    }
                }

                return sb.ToString();
            }


        }
        
        #endregion
    }
}
