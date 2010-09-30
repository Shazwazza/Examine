using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Examine;
using UmbracoExamine;
using Lucene.Net.Analysis;

namespace UmbracoExamine.Config
{
    /// <summary>
    /// This class is defined purely to maintain backwards compatibility
    /// </summary>
    [Obsolete("Use the new Examine.LuceneEngine.Config.IndexSets")]
    public class ExamineLuceneIndexes : Examine.LuceneEngine.Config.IndexSets
    {
    }

}

namespace UmbracoExamine
{
    /// <summary>
    /// This class purely exists to maintain backwards compatibility
    /// </summary>
    [Obsolete("Use the new UmbracoExamineSearcher instead")]
    public class LuceneExamineSearcher : UmbracoExamineSearcher
    {
        #region Constructors
        [Obsolete]
        public LuceneExamineSearcher()
            : base() { }
        [Obsolete]
        public LuceneExamineSearcher(DirectoryInfo indexPath)
            : base(indexPath) { }
		#endregion
    }

    /// <summary>
    /// This class purely exists to maintain backwards compatibility
    /// </summary>
    [Obsolete("Use the new UmbracoExamineIndexer instead")]
    public class LuceneExamineIndexer : UmbracoExamineIndexer
    {
        #region Constructors
        [Obsolete]
        public LuceneExamineIndexer()
            : base() { }
        [Obsolete]
        public LuceneExamineIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath)
            : base(indexerData, indexPath) { }
        #endregion
    }
}

namespace UmbracoExamine.SearchCriteria
{

    /// <summary>
    /// This exists purely to maintain backwards compatibility
    /// </summary>
    [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneSearchCriteria")]
    public class LuceneSearchCriteria : Examine.LuceneEngine.SearchCriteria.LuceneSearchCriteria
    {
        [Obsolete]
        public LuceneSearchCriteria(string type, Analyzer analyzer, string[] fields, bool allowLeadingWildcards, Examine.SearchCriteria.BooleanOperation occurance)
            : base(type, analyzer, fields, allowLeadingWildcards, occurance) { }
    }

    /// <summary>
    /// This exists purely to maintain backwards compatibility
    /// </summary>
    [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneBooleanOperation instead")]
    public class LuceneBooleanOperation : Examine.LuceneEngine.SearchCriteria.LuceneBooleanOperation
    {
        [Obsolete]
        public LuceneBooleanOperation(LuceneSearchCriteria search) : base(search) { }
    }

    /// <summary>
    /// This exists purely to maintain backwards compatibility
    /// </summary>
    [Obsolete("Use the new Examine.LuceneEngine.SearchCriteria.LuceneQuery instead")]
    public class LuceneQuery : Examine.LuceneEngine.SearchCriteria.LuceneQuery
    {
        [Obsolete]
        public LuceneQuery(LuceneSearchCriteria search, Lucene.Net.Search.BooleanClause.Occur occurance)
            : base(search, occurance) { }
    }
}