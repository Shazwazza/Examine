namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Defines a suggester for a Lucene Index
    /// </summary>
    public class LuceneSuggesterDefinition : SuggesterDefinition
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the suggester</param>
        /// <param name="suggesterMode">Suggester Mode</param>
        /// <param name="sourceFields">Source Index Fields for the Suggester</param>
        /// <param name="directoryFactory">Directory Factory for Lucene Suggesters (Required by AnalyzingInfixSuggester)</param>
        public LuceneSuggesterDefinition(string name, string suggesterMode, string[] sourceFields = null, ISuggesterDirectoryFactory directoryFactory = null)
            : base(name, suggesterMode, sourceFields)
        {
            SuggesterDirectoryFactory = directoryFactory;
        }

        /// <summary>
        /// Directory Factory for Lucene Suggesters
        /// </summary>
        public ISuggesterDirectoryFactory SuggesterDirectoryFactory { get; }
    }
}
