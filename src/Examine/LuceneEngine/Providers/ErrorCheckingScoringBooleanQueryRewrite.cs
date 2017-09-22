using System;
using System.Security;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Providers
{
    /// <summary>
    /// This is a work around for the TooManyClauses exception when rewriting wildcard queries
    /// </summary>
    /// <remarks>
    /// If a user wishes to turn on rewriting for wildcard queries and doesn't care about performance implications
    /// and to automatically just ignore these exceptions and use the default rewriter (non scoring), this syntax can be used:    
    /// <example>
    /// <![CDATA[
    /// var criteria = searcher.CreateSearchCriteria(); 
    /// var luceneCriteria = (LuceneSearchCriteria)criteria;
    /// luceneCriteria.QueryParser.SetMultiTermRewriteMethod(BaseLuceneSearcher.ErrorCheckingScoringBooleanQueryRewriteInstance);
    /// //Continue using the 'criteria' object to build up the query
    /// ]]>
    /// </example>
    /// </remarks>
    [SecurityCritical]
    [Serializable]
    public class ErrorCheckingScoringBooleanQueryRewrite : MultiTermQuery.RewriteMethod
    {
        [SecurityCritical]
        public override Query Rewrite(IndexReader reader, MultiTermQuery query)
        {
            //we'll try to use the SCORING_BOOLEAN_QUERY_REWRITE but this can result in TooManyClauses
            //which we need to handle. This might not be the greatest solution but its a work around for now.
            //see https://github.com/Shazwazza/Examine/pull/89
            //In newer lucene versions there's a top docs rewrite which doesn't have this problem but this looks like
            //an enormous amount of work to backport.
            //potentially we could some how bubble up the original query that has generated too many term matches so that 
            //the consumer could modify their search accordingly. 
            //another option would be to use the commented out code below and catch `booleanQuery.Add` and exit the loop when the
            //max terms are surpassed - but that might mean odd results.

            var baseClass = MultiTermQuery.SCORING_BOOLEAN_QUERY_REWRITE;
            try
            {
                var result = baseClass.Rewrite(reader, query);
                return result;
            }
            catch (BooleanQuery.TooManyClauses)
            {
                //TODO: We could try to bubble this up to the consumer somehow? event or otherwise?
                //TODO: We could add a cache for known terms that will cause this so that we don't spend too much CPU rewriting and recatching the exception each time

                //we cannot perform this rewrite so we need to use the default for this query
                var defaultRewriter = MultiTermQuery.CONSTANT_SCORE_AUTO_REWRITE_DEFAULT;
                var result = defaultRewriter.Rewrite(reader, query);
                return result;
            }


            //NOTE: this is the code that normally runs

            //var filteredTermEnum = query.GetEnum(reader);
            //var booleanQuery = new BooleanQuery(true);
            //var inc = 0;
            //try
            //{
            //    do
            //    {
            //        var t = filteredTermEnum.Term();
            //        if (t != null)
            //        {
            //            var termQuery = new TermQuery(t);
            //            termQuery.SetBoost(query.GetBoost() * filteredTermEnum.Difference());
            //
            //            NOTE: this is where the TooManyClauses Exception would occur 
            //
            //            booleanQuery.Add(termQuery, BooleanClause.Occur.SHOULD);
            //            ++inc;
            //        }
            //    }
            //    while (filteredTermEnum.Next());
            //}
            //finally
            //{
            //    filteredTermEnum.Close();
            //}
            //
            // NOTE: this is internal/protected, so if we wanted to use this code we'd have to subclass BooleanQuery
            //
            //query.IncTotalNumberOfTerms(inc);
            //return booleanQuery;



        }
    }
}