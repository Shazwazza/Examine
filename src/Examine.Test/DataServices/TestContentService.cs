using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Xml.XPath;
using NUnit.Framework;

namespace Examine.Test.DataServices
{

    /// <summary>
    /// A mock data service used to return content from the XML data file created with CWS
    /// </summary>
    public class TestContentService 
    {
        private XDocument _xDoc;

        /// <summary>
        /// Return the XDocument containing the xml from the umbraco.config xml file
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public XDocument GetPublishedContentByXPath(string xpath)
        {
            if (_xDoc == null)
            {
                var xmlFile = new DirectoryInfo(TestContext.CurrentContext.TestDirectory).GetDirectories("App_Data")
                    .Single()
                    .GetFiles("umbraco.config")
                    .Single();

                _xDoc = XDocument.Load(xmlFile.FullName);
            } 

            var xdoc = XDocument.Parse("<content></content>");
            xdoc.Root.Add(_xDoc.XPathSelectElements(xpath));

            return xdoc;
        }
    }
}
