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

namespace Examine.Test.DataServices
{

    /// <summary>
    /// A mock data service used to return content from the XML data file created with CWS
    /// </summary>
    public class TestContentService : IContentService
    {
        public TestContentService()
        {
            var xmlFile = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.GetDirectories("App_Data")
                .Single()
                .GetFiles("umbraco.config")
                .Single();

            m_XDoc = XDocument.Load(xmlFile.FullName);
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
            xdoc.Root.Add(m_XDoc.XPathSelectElements(xpath));

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
            xdoc.Root.Add(m_XDoc.XPathSelectElements(xpath));

            return xdoc;
        }

        public string StripHtml(string value)
        {
            string pattern = @"<(.|\n)*?>";
            return Regex.Replace(value, pattern, string.Empty);
        }

        public bool IsProtected(int nodeId, string path)
        {
            return false;
        }

        #endregion

        private XDocument m_XDoc;
    }
}
