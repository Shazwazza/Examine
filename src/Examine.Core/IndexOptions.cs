namespace Examine
{
    public class IndexOptions
    {
        public IndexOptions()
        {
            FieldDefinitions = new FieldDefinitionCollection();
            SimilarityDefinitions = new SimilarityDefinitionCollection();
        }

        public FieldDefinitionCollection FieldDefinitions { get; set; }

        public SimilarityDefinitionCollection SimilarityDefinitions { get; set; }

        public IValueSetValidator Validator { get; set; }
    }
}
