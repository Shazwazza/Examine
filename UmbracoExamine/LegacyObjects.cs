using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Examine;

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
        public LuceneExamineSearcher()
            : base() { }
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
        public LuceneExamineIndexer()
            : base() { }
        public LuceneExamineIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath)
            : base(indexerData, indexPath) { }
        #endregion
    }
}