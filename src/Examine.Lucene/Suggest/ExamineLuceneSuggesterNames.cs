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
        public const string AnalyzingSuggester = "Lucene.AnalyzingSuggester";

        /// <summary>
        /// Lucene.NET FuzzySuggester Suggester
        /// </summary>
        public const string FuzzySuggester = "Lucene.FuzzySuggester";

        /// <summary>
        /// Lucene.NET DirectSpellChecker with default string distance implementation.
        /// </summary>
        public const string DirectSpellChecker = "Lucene.DirectSpellChecker";


        /// <summary>
        /// Lucene.NET DirectSpellChecker with JaroWinklerDistance string distance implementation.
        /// </summary>
        public const string DirectSpellChecker_JaroWinklerDistance = "Lucene.DirectSpellChecker.JaroWinklerDistance";


        /// <summary>
        /// Lucene.NET DirectSpellChecker with LevensteinDistance string distance implementation.
        /// </summary>
        public const string DirectSpellChecker_LevensteinDistance = "Lucene.DirectSpellChecker.LevensteinDistance";

        /// <summary>
        /// Lucene.NET DirectSpellChecker with NGramDistance string distance implementation.
        /// </summary>
        public const string DirectSpellChecker_NGramDistance = "Lucene.DirectSpellChecker.NGramDistance";
    }
}
