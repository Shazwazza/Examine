using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace Examine.LuceneEngine.Providers
{
    ///<summary>
    /// A provider that allows for searching across multiple indexes
    ///</summary>
    public class MultiIndexSearcher : BaseLuceneSearcher
    {

        ///<summary>
        /// The index sets to search across
        ///</summary>
        public IEnumerable<IndexSet> SearchableIndexSets {get; protected internal set;}
        
        ///<summary>
        /// The underlying LuceneSearchers that will be searched across
        ///</summary>
        public IEnumerable<LuceneSearcher> Searchers { get; private set; }
        
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            //need to check if the index set is specified, if it's not, we'll see if we can find one by convension
            //if the folder is not null and the index set is null, we'll assume that this has been created at runtime.
            if (config["indexSets"] == null)
            {
                throw new ArgumentNullException("indexSets on MultiIndexSearcher provider has not been set in configuration");
            }

            var toSearch = new List<IndexSet>();
            var sets = IndexSets.Instance.Sets.Cast<IndexSet>();
            foreach(var i in config["indexSets"].Split(','))
            {
                var s = sets.Where(x => x.SetName == i).SingleOrDefault();
                if (s == null)
                {
                    throw new ArgumentException("The index set " + i + " does not exist");
                }
                toSearch.Add(s);
            }

            SearchableIndexSets = toSearch;

            //create the searchers
            Searchers = SearchableIndexSets.Select(s => new LuceneSearcher(s.IndexDirectory)).ToList();
        }
        
        /// <summary>
        /// Returns a list of fields to search on based on all distinct fields found in the sub searchers
        /// </summary>
        /// <returns></returns>
        protected override internal string[] GetSearchFields()
        {
            var searchableFields = new List<string>();
            foreach (var searcher in Searchers)
            {
                searchableFields.AddRange(searcher.GetSearchFields());
            }
            return searchableFields.Distinct().ToArray();
        }

        /// <summary>
        /// Gets the searcher for this instance
        /// </summary>
        /// <returns></returns>
        public override Searcher GetSearcher()
        {
            //validate all internal searchers
            foreach (var s in Searchers)
            {
                s.ValidateSearcher(false);
            }

            return new MultiSearcher(Searchers.Select(x => x.GetSearcher()).ToArray());
        }

     
    }
}
