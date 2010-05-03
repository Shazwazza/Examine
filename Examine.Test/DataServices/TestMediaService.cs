using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.DataServices;
using System.Xml.Linq;

namespace Examine.Test.DataServices
{
    public class TestMediaService : IMediaService
    {

        public TestMediaService()
        {
            //TODO: Create xml file for media test
            m_Doc = new XDocument(
                new XElement("root",
                    new XElement("node",
                        new XAttribute("id", 123),
                        new XAttribute("level", 1),
                        new XAttribute("nodeName", "test1"),
                        new XElement("node",
                            new XAttribute("id", 223),
                            new XAttribute("level", 2),
                            new XAttribute("nodeName", "test2")),
                        new XElement("node",
                            new XAttribute("id", 224),
                            new XAttribute("level", 2),
                            new XAttribute("nodeName", "test3")),
                        new XElement("node",
                            new XAttribute("id", 225),
                            new XAttribute("level", 2),
                            new XAttribute("nodeName", "test4")))));
        }

        #region IMediaService Members

        public System.Xml.Linq.XDocument GetLatestMediaByXpath(string xpath)
        {
            return m_Doc;
        }

        #endregion

        private XDocument m_Doc;
    }
}
