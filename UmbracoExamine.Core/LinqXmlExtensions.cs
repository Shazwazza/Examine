using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using umbraco;
using System.Xml;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;

namespace UmbracoExamine.Core
{
    /// <summary>
    /// Static methods to help query umbraco xml
    /// </summary>
    public static class LinqXmlExtensions
    {

        /// <summary>
        /// Converts a content node to XDocument
        /// </summary>
        /// <param name="node"></param>
        /// <param name="cacheOnly">true if data is going to be returned from cache</param>
        /// <returns></returns>
        /// <remarks>
        /// If the type of node is not a Document, the cacheOnly has no effect, it will use the API to return
        /// the xml. 
        /// </remarks>
        public static XDocument ToXDocument(this Content node, bool cacheOnly)
        {
            if (cacheOnly && node.GetType().Equals(typeof(Document)))
            {
                var umbXml = library.GetXmlNodeById(node.Id.ToString());
                return umbXml.ToXDocument();
            }

            //if it's not a using cache and it's not cacheOnly, then retrieve the Xml using the API

            XmlDocument xDoc = new XmlDocument();
            var xNode = xDoc.CreateNode(XmlNodeType.Element, "node", "");
            node.XmlPopulate(xDoc, ref xNode, false);
            return xDoc.ToXDocument();
        }

        /// <summary>
        /// Based on this blog, this is the fastest way to convert XmlDocument to XDocument
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static XDocument ToXDocument(this XmlDocument doc)
        {
            return XDocument.Load(new XmlNodeReader(doc));
        }

        public static XElement ToXElement(this XmlNode node)
        {
            return XElement.Load(new XmlNodeReader(node));
        }

        /// <summary>
        /// Converts an umbraco library call to an XDocument
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static XDocument ToXDocument(this XPathNodeIterator xml)
        {
            if (xml.Count == 1)
            {
                //ensure its readable.
                if (xml.MoveNext())
                {
                    //if ever the id is -1 then it's returned the whole tree which means its not found
                    //TODO: This is bug with older umbraco versions, i'm fairly sure it's fixed in new ones
                    //but just in case, we'll keep this here.
                    if (xml.Current.InnerXml.StartsWith("<root"))
                        return null;

                    return XDocument.Load(xml.Current.ReadSubtree());
                }
                return null;
            }
            else if (xml.Count > 1)
            {
                //create an XDocument and add a node to it
                XDocument xDoc = new XDocument(new XElement("nodes"));
                var rootNode = xDoc.Elements().First();

                //Import all elements from umbraco to the root node
                while (xml.MoveNext())
                {
                    rootNode.Add(XElement.Parse(xml.Current.OuterXml));
                }


                return xDoc;
            }

            return null;
        }

        /// <summary>
        /// Select the umbraco nodes within the xml document
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> UmbSelectNodes(this XDocument xml)
        {
            return xml.Descendants("node");
        }

        /// <summary>
        /// Returns a node by XPath.
        /// 
        /// Examples:
        /// GetNodeByXpath("//node[data[@alias='ShowInFooterMenu']/text()=1]")
        /// GetNodeByXpath("//node[@id='{0}']/ancestor-or-self::node[@level='3']")
        /// GetNodeByXpath("//node[@nodeTypeAlias='Home Page']")
        /// 
        /// </summary>
        /// <param name="xPath"></param>
        /// <returns></returns>
        public static XElement GetNodeByXpath(string xPath)
        {
            XPathNodeIterator umbXml = umbraco.library.GetXmlNodeByXPath(xPath);
            return umbXml.ToXDocument().Elements().First();
        }

        /// <summary>
        /// Returns umbraco NODE xml elements of a specific type
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="nodeTypeAlias"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> UmbSelectNodesWhereNodeTypeAlias(this IEnumerable<XElement> xml, string nodeTypeAlias)
        {
            return xml.Where(x => (string)x.Attribute("nodeTypeAlias") == nodeTypeAlias);
        }


        /// <summary>
        /// Returns true if current node is of type nodeTypeAlias. Else returns false.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="nodeTypeAlias"></param>
        /// <returns></returns>
        public static bool UmbIsNodeTypeAlias(this XElement xml, string nodeTypeAlias)
        {
            return ((string)xml.Attribute("nodeTypeAlias") == nodeTypeAlias);
        }


        /// <summary>
        /// Returns umbraco DATA xml elements of a specific type
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> UmbSelectDataWhereAlias(this IEnumerable<XElement> xml, string alias)
        {
            return xml.DescendantsAndSelf("data").Where(x => (string)x.Attribute("alias") == alias);
        }


        /// <summary>
        /// Returns umbraco NODE xml elements that have a data element with the specified alias
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> UmbSelectNodesWhereDataAlias(this IEnumerable<XElement> xml, string alias)
        {
            return xml.DescendantsAndSelf("node")
                .Where(x => x.Elements("data").Where(d => (string)d.Attribute("alias") == alias).Count() > 0);
        }

        /// <summary>
        /// Returns umbraco value for a data element with the specified alias. The XElement can either be a NODE or a DATA node and it will still work.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="alias"></param>
        /// <param name="valueIfNull"></param>
        /// <returns>If not found or the value is empty, returns the string value of the object passed as valueIfNull</returns>
        public static string UmbSelectDataValue(this XElement xml, string alias, object valueIfNull)
        {
            IEnumerable<XElement> val;

            if (xml.Name == "data")
            {
                val = xml.DescendantsAndSelf("data")
                    .UmbSelectDataWhereAlias(alias);
            }
            else
            {
                val = xml.Elements("data")
                    .UmbSelectDataWhereAlias(alias);
            }

            string strVal = val.DefaultIfEmpty(XElement.Parse(string.Format("<node>{0}</node>", valueIfNull.ToString()))) //ensures no error is thrown if no node is found
                            .First()
                            .Value;

            if (string.IsNullOrEmpty(strVal))
                return valueIfNull.ToString();
            return strVal;
        }

        /// <summary>
        /// Returns a typed object using the designated Converter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <param name="alias"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public static T UmbSelectDataValue<T>(this XElement xml, string alias, Converter<string, T> converter)
        {
            string val = UmbSelectDataValue(xml, alias, "");
            return converter.Invoke(val);
        }

        /// <summary>
        /// Returns umbraco value for a data element with the specified alias
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="alias"></param>
        /// <returns>If not found, returns an empty string</returns>
        public static string UmbSelectDataValue(this XElement xml, string alias)
        {
            return UmbSelectDataValue(xml, alias, "");
        }

    }
}
