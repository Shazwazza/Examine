using Lucene.Net.Index;
using Examine.Lucene.Indexing;
using Examine.Suggest;

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
        public LuceneSuggesterDefinition(string name, string[] sourceFields = null, ISuggesterDirectoryFactory directoryFactory = null)
            : base(name, sourceFields)
        {
            SuggesterDirectoryFactory = directoryFactory;
        }

        /// <summary>
        /// Directory Factory for Lucene Suggesters
        /// </summary>
        public ISuggesterDirectoryFactory SuggesterDirectoryFactory { get; }

        protected IIndexFieldValueType GetFieldValueType(FieldValueTypeCollection fieldValueTypeCollection, string fieldName)
        {
            //Get the value type for the field, or use the default if not defined
            return fieldValueTypeCollection.GetValueType(
                fieldName,
                fieldValueTypeCollection.ValueTypeFactories.GetRequiredFactory(FieldDefinitionTypes.FullText));
        }

        public abstract ILookupExecutor BuildSuggester(FieldValueTypeCollection fieldValueTypeCollection, ReaderManager readerManager, bool rebuild);

        public abstract ISuggestionResults ExecuteSuggester(string searchText, ISuggestionExecutionContext suggestionExecutionContext);
    }
}
