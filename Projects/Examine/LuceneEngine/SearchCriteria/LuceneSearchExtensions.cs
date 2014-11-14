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
        public static ILuceneSearcher GetLuceneSearcher(this ExamineManager mgr, string searcherName)
        {
            return (ILuceneSearcher)mgr.SearchProviderCollection[searcherName];
        }

        internal static IEnumerable<IndexReader> GetAllSubReaders(this IndexReader reader)
        {
            var readers = new List<IndexReader>();
            ReaderUtil.GatherSubReaders(readers, reader);
            return readers;
        }

        internal static IEnumerable<IndexSearcher> GetSubSearchers(this Searchable s)
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

        /// <summary>
        /// Converts an Examine boolean operation to a Lucene representation
        /// </summary>
        /// <param name="o">The operation.</param>
        /// <returns>The translated Boolean operation</returns>        
        public static Occur ToLuceneOccurrence(this BooleanOperation o)
        {
            switch (o)
            {
                case BooleanOperation.And:
                    return Occur.MUST;
                case BooleanOperation.Not:
                    return Occur.MUST_NOT;
                case BooleanOperation.Or:
                default:
                    return Occur.SHOULD;
            }
        }

        /// <summary>
        /// Converts a Lucene boolean occurrence to an Examine representation
        /// </summary>
        /// <param name="o">The occurrence to translate.</param>
        /// <returns>The translated boolean occurrence</returns>
        public static BooleanOperation ToBooleanOperation(this Occur o)
        {
            if (Equals(o, Occur.MUST))
            {
                return BooleanOperation.And;
            }
            else if (Equals(o, Occur.MUST_NOT))
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
