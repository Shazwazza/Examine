namespace Examine
{
    public class IndexOptions
    {
        public IndexOptions() {
            FieldDefinitions = new FieldDefinitionCollection();
            RelevanceScorerDefinitions = new RelevanceScorerDefinitionCollection();
        }

        public FieldDefinitionCollection FieldDefinitions { get; set; }
        public IValueSetValidator Validator { get; set; }

        public RelevanceScorerDefinitionCollection RelevanceScorerDefinitions { get; set; }
    }
}
