using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Examine;
using Examine.Providers;
using Examine.SearchCriteria;
using UmbracoExamine.Config;
using Examine.LuceneEngine;


namespace UmbracoExamine
{
    /// <summary>
    /// An Examine searcher which uses Lucene.Net as the 
    /// </summary>
	public class UmbracoExamineSearcher : Examine.LuceneEngine.LuceneExamineSearcher
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
