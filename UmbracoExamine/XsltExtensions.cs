using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using Examine;
using UmbracoExamine.DataServices;
using Examine.Providers;
using System.Linq;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Examine.LuceneEngine.Providers;

namespace UmbracoExamine
{
    /// <summary>
    /// Methods to support Umbraco XSLT extensions.
    /// </summary>
    /// <remarks>
    /// XSLT extensions will ONLY work for provider that have a base class of BaseUmbracoIndexer
    /// </remarks>
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
        /// Uses the provider specified to search, returning an XPathNodeIterator
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static XPathNodeIterator Search(string searchText, bool useWildcards, string providerName)
        {
            EnsureProvider(ExamineManager.Instance.SearchProviderCollection[providerName]);

            ISearchResults results = ExamineManager.Instance.SearchProviderCollection[providerName].Search(searchText, useWildcards);

            return GetResultsAsXml(results);            
        }

        /// <summary>
        /// Uses the default provider specified to search, returning an XPathNodeIterator
        /// </summary>
        /// <param name="searchText">The search query</param>
        /// <param name="useWildcards">Enable a wildcard search query</param>
        /// <returns>A node-set of the search results</returns>
        public static XPathNodeIterator Search(string searchText, bool useWildcards)
        {
            return Search(searchText, useWildcards, ExamineManager.Instance.DefaultSearchProvider.Name);
        }

        /// <summary>
        /// Will perform a search against the media index type only
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static XPathNodeIterator SearchMediaOnly(string searchText, bool useWildcards, string providerName)
        {
            EnsureProvider(ExamineManager.Instance.SearchProviderCollection[providerName]);

            var provider = ExamineManager.Instance.SearchProviderCollection[providerName] as LuceneSearcher;

            var results = provider.Search(searchText, useWildcards, IndexTypes.Media);

            return GetResultsAsXml(results);
        }

        /// <summary>
        /// Will perform a search against the media index type only
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <returns></returns>
        public static XPathNodeIterator SearchMediaOnly(string searchText, bool useWildcards)
        {
            return SearchMediaOnly(searchText, useWildcards, ExamineManager.Instance.DefaultSearchProvider.Name);
        }

        /// <summary>
        /// Will perform a search against the media index type only
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public static XPathNodeIterator SearchMediaOnly(string searchText)
        {
            return SearchMediaOnly(searchText, true);
        }

        /// <summary>
        /// Will perform a search against the content index type only
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static XPathNodeIterator SearchContentOnly(string searchText, bool useWildcards, string providerName)
        {
            EnsureProvider(ExamineManager.Instance.SearchProviderCollection[providerName]);

            var provider = ExamineManager.Instance.SearchProviderCollection[providerName] as LuceneSearcher;

            var results = provider.Search(searchText, useWildcards, IndexTypes.Content);

            return GetResultsAsXml(results);
        }

        /// <summary>
        /// Will perform a search against the content index type only
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="useWildcards"></param>
        /// <returns></returns>
        public static XPathNodeIterator SearchContentOnly(string searchText, bool useWildcards)
        {
            return SearchContentOnly(searchText, useWildcards, ExamineManager.Instance.DefaultSearchProvider.Name);
        }

        /// <summary>
        /// Will perform a search against the content index type only
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public static XPathNodeIterator SearchContentOnly(string searchText)
        {
            return SearchContentOnly(searchText, true);
        }


        private static void EnsureProvider(BaseSearchProvider p)
        {
            if (!(p is LuceneSearcher))
            {
                throw new NotSupportedException("XSLT Extensions are only support for providers of type LuceneSearcher");
            }
        }


        private static XPathNodeIterator GetResultsAsXml(ISearchResults results)
        {
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
