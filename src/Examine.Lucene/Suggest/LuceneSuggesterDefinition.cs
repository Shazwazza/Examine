using Lucene.Net.Index;
using Examine.Lucene.Indexing;
using Examine.Suggest;
using Lucene.Net.Analysis;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Defines a suggester for a Lucene Index
    /// </summary>
    public abstract class LuceneSuggesterDefinition : SuggesterDefinition
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the suggester</param>
        /// <param name="sourceFields">Source Index Fields for the Suggester</param>
        /// <param name="directoryFactory">Directory Factory for Lucene Suggesters (Required by AnalyzingInfixSuggester)</param>
        /// <param name="queryTimeAnalyzer">Analyzer to use at Query time</param>
        public LuceneSuggesterDefinition(string name, string[]? sourceFields, ISuggesterDirectoryFactory? directoryFactory, Analyzer? queryTimeAnalyzer)
            : base(name, sourceFields)
        {
            SuggesterDirectoryFactory = directoryFactory;
            QueryTimeAnalyzer = queryTimeAnalyzer;
        }

        /// <summary>
        /// Directory Factory for Lucene Suggesters
        /// </summary>
        public ISuggesterDirectoryFactory? SuggesterDirectoryFactory { get; }

        /// <summary>
        /// Query Time Analyzer
        /// </summary>
        public Analyzer? QueryTimeAnalyzer { get; }

        /// <summary>
        /// Build the Suggester Lookup
        /// </summary>
        /// <param name="fieldValueTypeCollection">Index fields</param>
        /// <param name="readerManager">Index Reader Manager</param>
        /// <param name="rebuild">Whether the lookup is being rebuilt</param>
        /// <returns>Lookup Executor</returns>
        public abstract ILookupExecutor BuildSuggester(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild);

        /// <summary>
        /// Executes the Suggester
        /// </summary>
        /// <param name="searchText">Search Text</param>
        /// <param name="suggestionExecutionContext">Suggestion Context</param>
        /// <returns></returns>
        public abstract ISuggestionResults ExecuteSuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext);


        /// <summary>
        /// Gets the field value type of a field name
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="fieldValueTypeCollection"></param>
        /// <returns></returns>
        protected IIndexFieldValueType GetFieldValueType(FieldValueTypeCollection fieldValueTypeCollection, string fieldName)
        {
            //Get the value type for the field, or use the default if not defined
            return fieldValueTypeCollection.GetValueType(
                fieldName,
                fieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));
        }
    }
}
