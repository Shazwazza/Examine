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
            if (string.IsNullOrWhiteSpace(termStr)) throw new System.ArgumentException($"'{nameof(termStr)}' cannot be null or whitespace", nameof(termStr));

            return GetFuzzyQuery(field, termStr, minSimilarity);
        }

        public virtual Query GetWildcardQueryInternal(string field, string termStr)
        {
            if (string.IsNullOrWhiteSpace(termStr)) throw new System.ArgumentException($"'{nameof(termStr)}' cannot be null or whitespace", nameof(termStr));
            
            return GetWildcardQuery(field, termStr);
        }

        public virtual Query GetFieldQueryInternal(string field, string queryText)
        {
            if (string.IsNullOrWhiteSpace(queryText)) throw new System.ArgumentException($"'{nameof(queryText)}' cannot be null or whitespace", nameof(queryText));

            return GetFieldQuery(field, queryText);
        }
    }
}