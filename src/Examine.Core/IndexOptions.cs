namespace Examine
{
    public class IndexOptions
    {
        public IndexOptions()
        {
            FieldDefinitions = new FieldDefinitionCollection();
            SuggesterDefinitions = new SuggesterDefinitionCollection();
        }

        public FieldDefinitionCollection FieldDefinitions { get; set; }
        public IValueSetValidator Validator { get; set; }

        public SuggesterDefinitionCollection SuggesterDefinitions { get; set; }
    }
}
