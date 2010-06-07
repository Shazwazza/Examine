using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.DataServices;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Xml.XPath;

namespace Examine.Test.DataServices
{
    public class TestMediaService : IMediaService
    {

        public TestMediaService()
        {
            var xmlFile = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.GetDirectories("App_Data")
                 .Single()
                 .GetFiles("media.xml")
                 .Single();

            m_Doc = XDocument.Load(xmlFile.FullName);
        }

        #region IMediaService Members

        public System.Xml.Linq.XDocument GetLatestMediaByXpath(string xpath)
        {
            var xdoc = XDocument.Parse("<media></media>");
            xdoc.Root.Add(m_Doc.XPathSelectElements(xpath));

            return xdoc;
        }

        #endregion

        private XDocument m_Doc;
    }
}
