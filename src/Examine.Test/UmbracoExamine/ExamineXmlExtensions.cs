using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Examine.Test.UmbracoExamine
{
    internal static class ExamineXmlExtensions
    {
        /// <summary>
        /// Converts an <see cref="System.Xml.XmlNode"/> to a <see cref="System.Xml.Linq.XElement"/>
        /// </summary>
        /// <param name="node">Node to convert</param>
        /// <returns>Converted node</returns>
        public static XElement ToXElement(this XmlNode node)
        {
            using (var x = new XmlNodeReader(node))
            {
                x.MoveToContent();
                return XElement.Load(x);
            }
        }

        /// <summary>
        /// Creates an <see cref="System.Xml.Linq.XDocument"/> from the collection of <see cref="System.Xml.Linq.XElement"/>
        /// </summary>
        /// <param name="elements">Elements to create document from</param>
        /// <returns>Document containing elements</returns>
        public static XDocument ToXDocument(this IEnumerable<XElement> elements)
        {
            if (elements.Any())
            {
                return new XDocument(new XElement("nodes", elements));
            }
            return null;
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
                    var tempNav = xml.Current.CreateNavigator();
                    if (tempNav.MoveToFirstChild())
                    {
                        if (tempNav.LocalName == "root")
                            return null;
                    }

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
                    rootNode.Add(XElement.Load(xml.Current.ReadSubtree()));
                }

                return xDoc;
            }

            return null;
        }
    }
}