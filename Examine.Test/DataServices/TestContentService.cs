using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.DataServices;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using UmbracoExamine.Config;
using UmbracoExamine;

namespace Examine.Test.DataServices
{

    /// <summary>
    /// A mock data service used to return content from the XML data file created with CWS
    /// </summary>
    public class TestContentService : IContentService
    {
        public const int ProtectedNode = 1142;

        public TestContentService()
        {
            var xmlFile = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.GetDirectories("App_Data")
                .Single()
                .GetFiles("umbraco.config")
                .Single();

            _xDoc = XDocument.Load(xmlFile.FullName);
        }

        #region IContentService Members

        /// <summary>
        /// Return the XDocument containing the xml from the umbraco.config xml file
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is no different in the test suite as published content
        /// </remarks>
        public XDocument GetLatestContentByXPath(string xpath)
        {
            var xdoc = XDocument.Parse("<content></content>");
            xdoc.Root.Add(_xDoc.XPathSelectElements(xpath));

            return xdoc;
        }

        /// <summary>
        /// Return the XDocument containing the xml from the umbraco.config xml file
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public XDocument GetPublishedContentByXPath(string xpath)
        {
            var xdoc = XDocument.Parse("<content></content>");
            xdoc.Root.Add(_xDoc.XPathSelectElements(xpath));

            return xdoc;
        }

        public string StripHtml(string value)
        {
            const string pattern = @"<(.|\n)*?>";
            return Regex.Replace(value, pattern, string.Empty);
        }

        public bool IsProtected(int nodeId, string path)
        {
            // single node is marked as protected for test indexer
            // hierarchy is not important for this test
            return nodeId == ProtectedNode;
        }

        public IEnumerable<string> GetAllUserPropertyNames()
        {
            return GetPublishedContentByXPath("//*[count(@id)>0]")
                .Root
                .Elements()
                .Select(x => x.Name.LocalName)
                .ToList();
        }

        public IEnumerable<string> GetAllSystemPropertyNames()
        {
            return UmbracoContentIndexer.IndexFieldPolicies.Select(x => x.Key);
        }

        #endregion

        private readonly XDocument _xDoc;







    }
}
