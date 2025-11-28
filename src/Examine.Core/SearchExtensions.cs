using System;
using Examine.Search;

namespace Examine
{
    /// <summary>
    /// A set of helpers for working with Lucene.Net in Examine
    /// </summary>
    public static class SearchExtensions
    {
        /// <summary>
        /// Adds a single character wildcard to the string for Lucene wildcard matching
        /// </summary>
        /// <param name="s">The string to wildcard.</param>
        /// <returns>An IExamineValue for the required operation</returns>
        /// <exception cref="System.ArgumentException">Thrown when the string is null or empty</exception>
        public static IExamineValue SingleCharacterWildcard(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("Supplied string is null or empty.", nameof(s));
            }

            return ExamineValue.Create(Examineness.SimpleWildcard, s);
        }

        /// <summary>
        /// Adds a multi-character wildcard to a string for Lucene wildcard matching
        /// </summary>
        /// <param name="s">The string to wildcard.</param>
        /// <returns>An IExamineValue for the required operation</returns>
        /// <exception cref="System.ArgumentException">Thrown when the string is null or empty</exception>
        public static IExamineValue MultipleCharacterWildcard(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("Supplied string is null or empty.", nameof(s));
            }

            return ExamineValue.Create(Examineness.ComplexWildcard, s);
        }

        /// <summary>
        /// Configures the string for fuzzy matching in Lucene using the default fuzziness level
        /// </summary>
        /// <param name="s">The string to configure fuzzy matching on.</param>
        /// <returns>An IExamineValue for the required operation</returns>
        /// <exception cref="System.ArgumentException">Thrown when the string is null or empty</exception>
        public static IExamineValue Fuzzy(this string s) => Fuzzy(s, 0.5f);

        /// <summary>
        /// Configures the string for fuzzy matching in Lucene using the supplied fuzziness level
        /// </summary>
        /// <param name="s">The string to configure fuzzy matching on.</param>
        /// <param name="fuzzieness">The fuzzieness level. A value between 0 and 2</param>
        /// <returns>
        /// An IExamineValue for the required operation
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown when the string is null or empty</exception>
        public static IExamineValue Fuzzy(this string s, float fuzzieness)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("Supplied string is null or empty.", nameof(s));
            }

            return ExamineValue.Create(Examineness.Fuzzy, s, fuzzieness);
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
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("Supplied string is null or empty.", nameof(s));
            }

            return ExamineValue.Create(Examineness.Boosted, s, boost);
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
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("Supplied string is null or empty.", nameof(s));
            }

            return ExamineValue.Create(Examineness.Proximity, s, Convert.ToSingle(proximity));
        }

        /// <summary>
        /// Escapes the string within Lucene
        /// </summary>
        /// <param name="s">The string to escape.</param>
        /// <returns>An IExamineValue for the required operation</returns>
        /// <exception cref="System.ArgumentException">Thrown when the string is null or empty</exception>
        [Obsolete("Use Phrase instead")]
        public static IExamineValue Escape(this string s) => Phrase(s);

        /// <summary>
        /// Ensures the string is treated as a phrase in Lucene
        /// </summary>
        /// <exception cref="System.ArgumentException">Thrown when the string is null or empty</exception>   
        public static IExamineValue Phrase(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("Supplied string is null or empty.", nameof(s));
            }

            //NOTE: You would be tempted to use QueryParser.Escape(s) here but that is incorrect because
            // inside of LuceneSearchCriteria when we detect Escaped, we use a PhraseQuery which automatically
            // escapes invalid chars.

            return ExamineValue.Create(Examineness.Phrase, s);
        }
    }
}
