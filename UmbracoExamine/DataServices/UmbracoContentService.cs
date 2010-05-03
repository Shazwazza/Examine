using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco;
using System.Xml.Linq;
using System.Xml;
using umbraco.cms.businesslogic.web;
using System.Collections;
using System.Xml.XPath;

namespace UmbracoExamine.DataServices
{
    public class UmbracoContentService : UmbracoExamine.DataServices.IContentService
    {

        /// <summary>
        /// removes html markup from a string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string StripHtml(string value)
        {
            return library.StripHtml(value);
        }

        /// <summary>
        /// Gets published content by xpath
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public XDocument GetPublishedContentByXPath(string xpath)
        {
            return library.GetXmlNodeByXPath(xpath).ToXDocument();
        }

        /// <summary>
        /// This is quite an intensive operation...
        /// get all root content, then get the XML structure for all children,
        /// then run xpath against the navigator that's created
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        public XDocument GetLatestContentByXPath(string xpath)
        {   

            var rootContent = Document.GetRootDocuments();
            var xmlContent = XDocument.Parse("<content></content>");
            var xDoc = new XmlDocument();
            foreach (var c in rootContent)
            {
                var xNode = xDoc.CreateNode(XmlNodeType.Element, "node", "");
                c.XmlPopulate(xDoc, ref xNode, true);

                if (xNode.Attributes["nodeTypeAlias"] == null)
                {
                    //we'll add the nodeTypeAlias ourselves                                
                    XmlAttribute d = xDoc.CreateAttribute("nodeTypeAlias");
                    d.Value = c.ContentType.Alias;
                    xNode.Attributes.Append(d);
                }

                xmlContent.Root.Add(xNode.ToXElement());
            }
            var result = ((IEnumerable)xmlContent.XPathEvaluate(xpath)).Cast<XElement>();
            return result.ToXDocument();
        }

        /// <summary>
        /// Unfortunately, we need to implement our own IsProtected method since 
        /// the Umbraco core code requires an HttpContext for this method and when we're running
        /// async, there is no context
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        private XmlNode GetPage(int documentId)
        {
            XmlNode x = Access.AccessXml.SelectSingleNode("/access/page [@id=" + documentId.ToString() + "]");
            return x;
        }

        /// <summary>
        /// Unfortunately, we need to implement our own IsProtected method since 
        /// the Umbraco core code requires an HttpContext for this method and when we're running
        /// async, there is no context
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsProtected(int nodeId, string path)
        {
            foreach (string id in path.Split(','))
            {
                if (GetPage(int.Parse(id)) != null)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
