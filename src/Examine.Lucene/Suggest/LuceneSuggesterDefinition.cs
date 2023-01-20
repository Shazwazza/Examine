namespace Examine.Lucene.Suggest
{
    public class LuceneSuggesterDefinition : SuggesterDefinition
    {
        public LuceneSuggesterDefinition(string name, string suggesterMode, string[] sourceFields = null, ISuggesterDirectoryFactory directoryFactory = null)
            : base(name, suggesterMode, sourceFields)
        {
            SuggesterDirectoryFactory = directoryFactory;
        }

        public ISuggesterDirectoryFactory SuggesterDirectoryFactory { get; }
    }
}
