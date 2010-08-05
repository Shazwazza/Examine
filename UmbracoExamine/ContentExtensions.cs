using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using umbraco;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;

namespace UmbracoExamine
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

            if (xNode.Attributes["nodeTypeAlias"] == null)
            {
                //we'll add the nodeTypeAlias ourselves                                
                XmlAttribute d = xDoc.CreateAttribute("nodeTypeAlias");
                d.Value = node.ContentType.Alias;
                xNode.Attributes.Append(d);
            }

            return new XDocument(xNode.ToXElement());
        }

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
            if (elements.Count() > 0)
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
                    rootNode.Add(XElement.Load(xml.Current.ReadSubtree()));
                }

                return xDoc;
            }

            return null;
        }

        /// <summary>
        /// Checks if the XElement is an umbraco property based on an alias.
        /// This works for both types of schemas
        /// </summary>
        /// <param name="x"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static bool UmbIsProperty(this XElement x, string alias)
        {
            if ((x.Name == alias) //this will match if its the new schema
                || (x.Name == "data" && (string)x.Attribute("nodeTypeAlias") == alias)) //this will match old schema
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the XElement is recognized as an umbraco xml NODE (doc type) 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static bool UmbIsElement(this XElement x)
        {
            return ((string)x.Attribute("id") != "" && int.Parse((string)x.Attribute("id")) > 0);
        }

        /// <summary>
        /// This takes into account both schemas and returns the node type alias.
        /// If this isn't recognized as an element node, this returns an empty string
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string UmbNodeTypeAlias(this XElement x)
        {
            return !x.UmbIsElement() ? string.Empty
                : string.IsNullOrEmpty(((string)x.Attribute("nodeTypeAlias"))) ? x.Name.LocalName
                : (string)x.Attribute("nodeTypeAlias");
        }

        /// <summary>
        /// Returns the property value for the doc type element (such as id, path, etc...)
        /// If the element is not an umbraco doc type node, or the property name isn't found, it returns String.Empty 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static string UmbSelectPropertyValue(this XElement x, string alias)
        {
            if (alias == "nodeTypeAlias")
            {
                return x.UmbNodeTypeAlias();
            }
            else
            {
                return (string)x.Attribute(alias);
            }
        }

        /// <summary>
        /// Returns umbraco value for a data element with the specified alias.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static string UmbSelectDataValue(this XElement xml, string alias)
        {
            if (!xml.UmbIsElement())
                return string.Empty;

            XElement nodeData = null;

           
            //if there is data children with attributes, we're on the old
            if (xml.Elements("data").Where(x => x.HasAttributes).Count() > 0)
            {
                nodeData = xml.Elements("data").SingleOrDefault(x => ((string)x.Attribute("alias")).ToUpper() == alias.ToUpper());
            }
            else
            {
                //find the element with the uppercased name (umbraco camel cases things in xml even if the alias isn't)
                nodeData = xml.Elements().Where(x => x.Name.ToString().ToUpper() == alias.ToUpper()).FirstOrDefault();
            }           
            
            if (nodeData == null)
            {
                return string.Empty;
            }
            else
            {
                return nodeData.Value;
            }

        }

    }
}
