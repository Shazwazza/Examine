namespace Examine
{
    /// <summary>
    /// Built in Suggester names for Examine Lucene
    /// </summary>
    public class ExamineLuceneSuggesterNames
    {
        /// <summary>
        /// Lucene.NET AnalyzingSuggester Suggester
        /// </summary>
        public const string AnalyzingSuggester = "LuceneAnalyzingSuggester";

        /// <summary>
        /// Lucene.NET DirectSpellChecker with default string distance implementation.
        /// </summary>
        public const string DirectSpellChecker = "LuceneDirectSpellChecker";


        /// <summary>
        /// Lucene.NET DirectSpellChecker with JaroWinklerDistance string distance implementation.
        /// </summary>
        public const string DirectSpellChecker_JaroWinklerDistance = "LuceneDirectSpellChecker|JaroWinklerDistance";


        /// <summary>
        /// Lucene.NET DirectSpellChecker with LevensteinDistance string distance implementation.
        /// </summary>
        public const string DirectSpellChecker_LevensteinDistance = "LuceneDirectSpellChecker|LevensteinDistance";


        /// <summary>
        /// Lucene.NET DirectSpellChecker with NGramDistance string distance implementation.
        /// </summary>
        public const string DirectSpellChecker_NGramDistance = "LuceneDirectSpellChecker|NGramDistance";
    }
}
