using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Examine.LuceneEngine.Indexing;

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
            return new XElement("node",
                //creates the element attributes
                new XAttribute("id", nodeId),
                new XAttribute("nodeTypeAlias", nodeType),
                    //creates the data nodes
                    data.Select(x => new XElement("data",
                        new XAttribute("alias", x.Key),
                        new XCData(x.Value))).ToList());
            
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
            return !x.IsExamineElement() ? string.Empty
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
        public static string SelectExaminePropertyValue(this XElement x, string alias)
        {
            if (alias == "nodeTypeAlias")
            {
                return x.ExamineNodeTypeAlias();
            }
            else
            {
                return (string)x.Attribute(alias);
            }
        }

        /// <summary>
        /// Converts the legacy XML representation to a ValueSet
        /// </summary>
        /// <param name="node"></param>
        /// <param name="itemType">
        /// The item's node type (in umbraco terms this would be the doc type alias)</param>
        /// <param name="indexCategory">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="id"></param>
        /// <returns></returns>
        //public static ValueSet ToValueSet(this XElement node, string type, long? id = null)
        public static ValueSet ToValueSet(this XElement node, string indexCategory, string itemType, long? id = null)
        {
            id = id ?? long.Parse((string)node.Attribute("id"));
            var set = new ValueSet(id.Value, indexCategory, node.ExamineNodeTypeAlias()) { OriginalNode = node};
            
            foreach (var attr in node.Attributes())
            {
                if (attr.Name != "id")
                {
                    set.Add(attr.Name.LocalName, attr.Value);                    
                }
            }

            var hadData = false;
            foreach (var d in node.Elements("data"))
            {
                var alias = (string)d.Attribute("alias");
                if (!string.IsNullOrEmpty(alias))
                {
                    set.Add(alias, d.Value);
                    hadData = true;
                }
            }

            if (!hadData)
            {
                foreach (var e in node.Elements())
                {
                    set.Add(e.Name.LocalName, e.Value);
                }
            }
            
            return set;
        }

        /// <summary>
        /// Returns umbraco value for a data element with the specified alias.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static string SelectExamineDataValue(this XElement xml, string alias)
        {
            if (!xml.IsExamineElement())
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
