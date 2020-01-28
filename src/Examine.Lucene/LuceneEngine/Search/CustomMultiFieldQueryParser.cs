using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace Examine.LuceneEngine.Search
{

    /// <summary>
    /// We use this to get at the protected methods directly since the new version makes them not public
    /// </summary>
    public class CustomMultiFieldQueryParser : MultiFieldQueryParser
    {

        public CustomMultiFieldQueryParser(Version matchVersion, string[] fields, Analyzer analyzer) : base(matchVersion, fields, analyzer)
        {
        }

        public virtual Query GetFuzzyQueryInternal(string field, string termStr, float minSimilarity)
        {
            return GetFuzzyQuery(field, termStr, minSimilarity);
        }

        public virtual Query GetWildcardQueryInternal(string field, string termStr)
        {
            return GetWildcardQuery(field, termStr);
        }

        public virtual Query GetFieldQueryInternal(string field, string queryText)
        {
            return GetFieldQuery(field, queryText);
        }
    }
}