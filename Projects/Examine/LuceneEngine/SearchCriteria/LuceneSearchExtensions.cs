using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.Scoring;
using Examine.SearchCriteria;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System.Linq;
using Lucene.Net.Util;

namespace Examine.LuceneEngine.SearchCriteria
{
    /// <summary>
    /// A set of helpers for working with Lucene.Net in Examine
    /// </summary>
    public static class LuceneSearchExtensions
    {
        /// <summary>
        /// Returns a BaseLuceneSearcher with the specified name. If the searcher is not a BaseLuceneSearcher
        /// an exception will be thrown.
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="searcherName"></param>
        /// <returns></returns>
        public static BaseLuceneSearcher GetLuceneSearcher(this ExamineManager mgr, string searcherName)
        {
            return (BaseLuceneSearcher)mgr.SearchProviderCollection[searcherName];
        }

        public static IEnumerable<IndexReader> GetAllSubReaders(this IndexReader reader)
        {
            var readers = new ArrayList();
            ReaderUtil.GatherSubReaders(readers, reader);
            return readers.Cast<IndexReader>();
        }
        
        public static IEnumerable<IndexSearcher> GetSubSearchers(this Searchable s)
        {
            var ixs = s as IndexSearcher;
            if (ixs != null)
            {
                yield return ixs;
            }

            var ms = s as MultiSearcher;
            if (ms != null)
            {
                foreach (var mss in ms.GetSearchables())
                {
                    foreach (var ss in mss.GetSubSearchers())
                    {
                        yield return ss;
                    }
                }
            }
        }
        
        ///// <summary>
        ///// Performs a true Lucene Query 
        ///// </summary>
        ///// <param name="searcher"></param>
        ///// <param name="query"></param>
        ///// <returns></returns>
        ///// <remarks>
        ///// so long as the searcher is a lucene searcher, otherwise an exception is thrown
        ///// </remarks>
        //public static ILuceneSearchResults LuceneSearch(this ISearcher searcher, Query query)
        //{
        //    var typedSearcher = (ISearcher<ILuceneSearchResults, LuceneSearchResult, LuceneSearchCriteria>)searcher;
        //    return searcher.LuceneSearch(typedSearcher.CreateCriteria().LuceneQuery(query).Compile());
        //}

        ///// <summary>
        ///// Searches and returns a typed lucene search result 
        ///// </summary>
        ///// <param name="searcher"></param>
        ///// <param name="criteria"></param>
        ///// <returns></returns>
        ///// <remarks>
        ///// so long as the searcher is a lucene searcher, otherwise an exception is thrown
        ///// </remarks>
        //public static ILuceneSearchResults LuceneSearch(this ISearcher searcher, LuceneSearchCriteria criteria)
        //{
        //    var typedSearcher = (ISearcher<ILuceneSearchResults, LuceneSearchResult, LuceneSearchCriteria>)searcher;
        //    return typedSearcher.Find(criteria);
        //}

        /// <summary>
        /// Used to order results by the specified fields 
        /// </summary>
        /// <param name="qry"></param>
        /// <param name="fields">The fields to sort by and the type to sort them on</param>
        /// <returns></returns>
        public static IBooleanOperation OrderBy(this IQuery qry, params SortableField[] fields)
        {
            return qry.OrderBy(
                fields.Select(x => x.FieldName + "[Type=" + x.SortType.ToString().ToUpper() + "]").ToArray());
        }

        /// <summary>
        /// Used to order results by the specified fields 
        /// </summary>
        /// <param name="qry"></param>
        /// <param name="fields">The fields to sort by and the type to sort them on</param>
        /// <returns></returns>
        public static IBooleanOperation OrderByDescending(this IQuery qry, params SortableField[] fields)
        {
            return qry.OrderByDescending(
                fields.Select(x => x.FieldName + "[Type=" + x.SortType.ToString().ToUpper() + "]").ToArray());
        }

        /// <summary>
        /// Adds a single character wildcard to the string for Lucene wildcard matching
        /// </summary>
        /// <param name="s">The string to wildcard.</param>
        /// <returns>An IExamineValue for the required operation</returns>
        /// <exception cref="System.ArgumentException">Thrown when the string is null or empty</exception>
        public static IExamineValue SingleCharacterWildcard(this string s)
        {
            if (System.String.IsNullOrEmpty(s))
                throw new ArgumentException("Supplied string is null or empty.", "s");

            return new ExamineValue(Examineness.SimpleWildcard, s + "?");
        }

        /// <summary>
        /// Adds a multi-character wildcard to a string for Lucene wildcard matching
        /// </summary>
        /// <param name="s">The string to wildcard.</param>
        /// <returns>An IExamineValue for the required operation</returns>
        /// <exception cref="System.ArgumentException">Thrown when the string is null or empty</exception>
        public static IExamineValue MultipleCharacterWildcard(this string s)
        {
            if (String.IsNullOrEmpty(s))
                throw new ArgumentException("Supplied string is null or empty.", "s");
            return new ExamineValue(Examineness.ComplexWildcard, s + "*");
        }

        /// <summary>
        /// Configures the string for fuzzy matching in Lucene using the default fuzziness level
        /// </summary>
        /// <param name="s">The string to configure fuzzy matching on.</param>
        /// <returns>An IExamineValue for the required operation</returns>
        /// <exception cref="System.ArgumentException">Thrown when the string is null or empty</exception>
        public static IExamineValue Fuzzy(this string s)
        {
            return Fuzzy(s, 0.5f);
        }

        /// <summary>
        /// Configures the string for fuzzy matching in Lucene using the supplied fuzziness level
        /// </summary>
        /// <param name="s">The string to configure fuzzy matching on.</param>
        /// <param name="fuzzyness">The fuzzyness level.</param>
        /// <returns>
        /// An IExamineValue for the required operation
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when the string is null or empty</exception>
        public static IExamineValue Fuzzy(this string s, float fuzzyness)
        {
            if (String.IsNullOrEmpty(s))
                throw new ArgumentException("Supplied string is null or empty.", "s");
            return new ExamineValue(Examineness.Fuzzy, s, fuzzyness);
        }

        /// <summary>
        /// Configures the string for boosting in Lucene
        /// </summary>
        /// <param name="s">The string to wildcard.</param>
        /// <param name="boost">The boost level.</param>
        /// <returns>
        /// An IExamineValue for the required operation
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when the string is null or empty</exception>
        public static IExamineValue Boost(this string s, float boost)
        {
            if (String.IsNullOrEmpty(s))
                throw new ArgumentException("Supplied string is null or empty.", "s");
            return new ExamineValue(Examineness.Boosted, s, boost);
        }

        /// <summary>
        /// Configures the string for proximity matching
        /// </summary>
        /// <param name="s">The string to wildcard.</param>
        /// <param name="proximity">The proximity level.</param>
        /// <returns>
        /// An IExamineValue for the required operation
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when the string is null or empty</exception>
        public static IExamineValue Proximity(this string s, int proximity)
        {
            if (String.IsNullOrEmpty(s))
                throw new ArgumentException("Supplied string is null or empty.", "s");
            return new ExamineValue(Examineness.Proximity, s, Convert.ToSingle(proximity));
        }

        /// <summary>
        /// Escapes the string within Lucene
        /// </summary>
        /// <param name="s">The string to escape.</param>
        /// <returns>An IExamineValue for the required operation</returns>
        /// <exception cref="System.ArgumentException">Thrown when the string is null or empty</exception>
        
        public static IExamineValue Escape(this string s)
        {
            if (String.IsNullOrEmpty(s))
                throw new ArgumentException("Supplied string is null or empty.", "s");

            //NOTE: You would be tempted to use QueryParser.Escape(s) here but that is incorrect because
            // inside of LuceneSearchCriteria when we detect Escaped, we use a PhraseQuery which automatically
            // escapes invalid chars.
            
            return new ExamineValue(Examineness.Escaped, s);
        }

        ///// <summary>
        ///// Sets up an <see cref="IExamineValue"/> for an additional Examiness
        ///// </summary>
        ///// <param name="examineValue">The IExamineValue to continue working with.</param>
        ///// <param name="s">The string to postfix.</param>
        ///// <returns>Combined strings</returns>
        //public static string Then(this IExamineValue examineValue, string s)
        //{
        //    if (examineValue == null)
        //        throw new ArgumentNullException("examineValue", "examineValue is null.");
        //    if (String.IsNullOrEmpty(s))
        //        throw new ArgumentException("Supplied string is null or empty.", "s");
        //    return examineValue.Value + s;
        //}

        ///// <summary>
        ///// Sets up an <see cref="IExamineValue"/> for an additional Examiness
        ///// </summary>
        ///// <param name="examineValue">The IExamineValue to continue working with.</param>
        ///// <returns>Combined strings</returns>
        //public static string Then(this IExamineValue examineValue)
        //{
        //    return Then(examineValue, string.Empty);
        //}

        /// <summary>
        /// Converts an Examine boolean operation to a Lucene representation
        /// </summary>
        /// <param name="o">The operation.</param>
        /// <returns>The translated Boolean operation</returns>
        
        public static BooleanClause.Occur ToLuceneOccurrence(this BooleanOperation o)
        {
            switch (o)
            {
                case BooleanOperation.And:
                    return BooleanClause.Occur.MUST;
                case BooleanOperation.Not:
                    return BooleanClause.Occur.MUST_NOT;
                case BooleanOperation.Or:
                default:
                    return BooleanClause.Occur.SHOULD;
            }
        }

        /// <summary>
        /// Converts a Lucene boolean occurrence to an Examine representation
        /// </summary>
        /// <param name="o">The occurrence to translate.</param>
        /// <returns>The translated boolean occurrence</returns>
        
        public static BooleanOperation ToBooleanOperation(this BooleanClause.Occur o)
        {
            if (Equals(o, BooleanClause.Occur.MUST))
            {
                return BooleanOperation.And;
            }
            else if (Equals(o, BooleanClause.Occur.MUST_NOT))
            {
                return BooleanOperation.Not;
            }
            else
            {
                return BooleanOperation.Or;
            }
        }
    }
}
