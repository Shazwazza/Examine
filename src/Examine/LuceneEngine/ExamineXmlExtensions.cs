using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Static methods to help query umbraco xml
    /// </summary>
    public static class ExamineXmlExtensions
    {
        /// <summary>
        /// Translates a dictionary object, node id, and node type into the property xml structure used by the examine indexer
        /// </summary>
        /// <param name="?"></param>
        /// <param name="data"></param>
        /// <param name="nodeId"></param>
        /// <param name="nodeType"></param>
        /// <returns>
        /// returns an XElement with the default Examine XML structure
        /// </returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <root>
        ///     <node id="1234" nodeTypeAlias="yourIndexType">
        ///         <data alias="fieldName1">Some data</data>
        ///         <data alias="fieldName2">Some other data</data>
        ///     </node>
        ///     <node id="345" nodeTypeAlias="anotherIndexType">
        ///         <data alias="fieldName3">More data</data>
        ///     </node>
        /// </root>
        /// ]]>
        /// </code>        
        /// </example>
        public static XElement ToExamineXml(this Dictionary<string, string> data, int nodeId, string nodeType)
        {
            var nodes = new List<XElement>();
            foreach (var x in data)
            {
                if (!string.IsNullOrWhiteSpace(x.Value))
                {
                    nodes.Add(new XElement("data",
                        new XAttribute("alias", x.Key),
                        new XCData(x.Value)));
                }
            }

            return new XElement("node",
                //creates the element attributes
                new XAttribute("id", nodeId),
                new XAttribute("nodeTypeAlias", nodeType),
                nodes);

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

        /// <summary>
        /// Checks if the XElement is an umbraco property based on an alias.
        /// This works for both types of schemas
        /// </summary>
        /// <param name="x"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static bool IsExamineProperty(this XElement x, string alias)
        {
            if ((x.Name == alias) //this will match if its the new schema
                || (x.Name == "data" && (string) x.Attribute("nodeTypeAlias") == alias)) //this will match old schema
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
        public static bool IsExamineElement(this XElement x)
        {
            var id = (string) x.Attribute("id");
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }
            int parsedId;
            if (int.TryParse(id, out parsedId))
            {
                if (parsedId > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This takes into account both schemas and returns the node type alias.
        /// If this isn't recognized as an element node, this returns an empty string
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string ExamineNodeTypeAlias(this XElement x)
        {
            return string.IsNullOrEmpty(((string) x.Attribute("nodeTypeAlias")))
                ? x.Name.LocalName
                : (string) x.Attribute("nodeTypeAlias");
        }

        /// <summary>
        /// Returns the property value for the doc type element (such as id, path, etc...)
        /// If the element is not an umbraco doc type node, or the property name isn't found, it returns String.Empty 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static string SelectExaminePropertyValue(this XElement x, string alias)
        {
            if (alias == "nodeTypeAlias")
            {
                return x.ExamineNodeTypeAlias();
            }
            else
            {
                return (string) x.Attribute(alias);
            }
        }

        internal static string SelectExaminePropertyValue(this XElement x, IDictionary<string, string> attributeValues, string alias)
        {
            if (alias == "nodeTypeAlias")
            {
                return x.ExamineNodeTypeAlias();
            }
            else
            {
                string result;
                attributeValues.TryGetValue(alias, out result);
                return result;
            }
        }

        /// <summary>
        /// Returns umbraco value for a data element with the specified alias.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static string SelectExamineDataValue(this XElement xml, string alias)
        {
            XElement nodeData = null;

            //if there is data children with attributes, we're on the old
            if (xml.Elements("data").Any(x => x.HasAttributes))
            {
                nodeData = xml.Elements("data").SingleOrDefault(x => string.Equals(((string) x.Attribute("alias")), alias, StringComparison.InvariantCultureIgnoreCase));
            }
            else
            {
                nodeData = xml.Elements().FirstOrDefault(x => string.Equals(x.Name.ToString(), alias, StringComparison.InvariantCultureIgnoreCase));
            }

            if (nodeData == null)
            {
                return string.Empty;
            }

            if (!nodeData.HasElements)
            {
                return nodeData.Value;
            }

            //it has sub elements so serialize them
            var reader = nodeData.CreateReader();
            reader.MoveToContent();
            return reader.ReadInnerXml();
        }

        internal static Dictionary<string, string> SelectExamineDataValues(this XElement xml)
        {
            //resolve all element data at once since it is much faster to do this than to relookup all of the XML data
            //using Linq and the node.Elements() methods re-gets all of them.
            var elementValues = new Dictionary<string, string>();
            foreach (var x in xml.Elements())
            {
                string key;
                if (x.Name.LocalName == "data")
                {
                    //it's the legacy schema
                    key = (string)x.Attribute("alias");
                }
                else
                {
                    key = x.Name.LocalName;
                }

                if (string.IsNullOrEmpty(key))
                    continue;

                if (!x.HasElements)
                {
                    elementValues[key] = x.Value;
                }
                else
                {
                    //it has sub elements so serialize them
                    using (var reader = x.CreateReader())
                    {
                        reader.MoveToContent();
                        elementValues[key] = reader.ReadInnerXml();
                    }
                }
            }
            return elementValues;
        }
    }
}
