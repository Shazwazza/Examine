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
        /// Returns a list of fields to search on, this will also exclude the IndexPathFieldName
        /// </summary>
        /// <returns></returns>
        protected override string[] GetSearchFields()
        {
            var fields = base.GetSearchFields();
            return fields.Where(x => x != UmbracoExamineIndexer.IndexPathFieldName).ToArray();
        }		
    }
}
