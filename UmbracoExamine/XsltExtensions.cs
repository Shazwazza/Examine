using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using Examine;
using System.Xml.Linq;

namespace UmbracoExamine
{
    /// <summary>
    /// Methods to support Umbraco XSLT extensions
    /// </summary>
    public class XsltExtensions
    {
       

        /// <summary>
        /// Uses the default provider specified to search, returning an XPathNodeIterator
        /// </summary>
        /// <param name="searchText">The search query</param>
        /// <returns>A node-set of the search results</returns>
        public static XPathNodeIterator Search(string searchText)
        {
            return Search(searchText, true);
        }

        /// <summary>
        /// Uses the default provider specified to search, returning an XPathNodeIterator
        /// </summary>
        /// <param name="searchText">The search query</param>
        /// <param name="useWildcards">Enable a wildcard search query</param>
        /// <returns>A node-set of the search results</returns>
        public static XPathNodeIterator Search(string searchText, bool useWildcards)
        {
            ISearchResults results = ExamineManager.Instance.Search(searchText, useWildcards);

            // create the XDocument
            XDocument doc = new XDocument();

            // check there are any search results
            if (results.TotalItemCount > 0)
            {
                // create the root element
                XElement root = new XElement("nodes");

                // iterate through the search results
                foreach (SearchResult result in results)
                {
                    // create a new <node> element
                    XElement node = new XElement("node");

                    // create the @id attribute
                    XAttribute nodeId = new XAttribute("id", result.Id);

                    // create the @score attribute
                    XAttribute nodeScore = new XAttribute("score", result.Score);

                    // add the content
                    node.Add(nodeId, nodeScore);

                    foreach (KeyValuePair<String, String> field in result.Fields)
                    {
                        // create a new <data> element
                        XElement data = new XElement("data");

                        // create the @alias attribute
                        XAttribute alias = new XAttribute("alias", field.Key);

                        // assign the value to a CDATA section
                        XCData value = new XCData(field.Value);

                        // append the content
                        data.Add(alias, value);

                        // append the <data> element
                        node.Add(data);
                    }

                    // add the node
                    root.Add(node);
                }

                // add the root node
                doc.Add(root);
            }
            else
            {
                doc.Add(new XElement("error", "There were no search results."));
            }

            return doc.CreateNavigator().Select("/");
        }

    }

}
