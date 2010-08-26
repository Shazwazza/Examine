using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Examine;
using Examine.Providers;
using Examine.SearchCriteria;
using UmbracoExamine.Config;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;


namespace UmbracoExamine
{
    /// <summary>
    /// An Examine searcher which uses Lucene.Net as the 
    /// </summary>
	public class UmbracoExamineSearcher : LuceneSearcher
    {

        #region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
        public UmbracoExamineSearcher()
            : base()
        {
        }
		
		/// <summary>
		/// Constructor to allow for creating an indexer at runtime
		/// </summary>
		/// <param name="indexPath"></param>
        public UmbracoExamineSearcher(DirectoryInfo indexPath)
            : base(indexPath)
        {
        }

		#endregion

        /// <summary>
        /// initializes the searcher
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
        }

        /// <summary>
        /// Override in order to set the nodeTypeAlias field name of the underlying SearchCriteria to __NodeTypeAlias
        /// </summary>
        /// <param name="type"></param>
        /// <param name="defaultOperation"></param>
        /// <returns></returns>
        public override ISearchCriteria CreateSearchCriteria(string type, BooleanOperation defaultOperation)
        {
            var criteria = base.CreateSearchCriteria(type, defaultOperation) as LuceneSearchCriteria;
            criteria.NodeTypeAliasField = UmbracoExamineIndexer.NodeTyepAliasFieldName;
            return criteria;
        }

        /// <summary>
        /// Returns a list of fields to search on, this will also exclude the IndexPathFieldName and node type alias
        /// </summary>
        /// <returns></returns>
        protected override string[] GetSearchFields()
        {
            var fields = base.GetSearchFields();
            return fields
                .Where(x => x != UmbracoExamineIndexer.IndexPathFieldName)
                .Where(x => x != UmbracoExamineIndexer.NodeTyepAliasFieldName)
                .ToArray();
        }		
    }
}
