namespace Examine
{
    public class IndexOptions
    {
        public IndexOptions() => FieldDefinitions = new FieldDefinitionCollection();

        public FieldDefinitionCollection FieldDefinitions { get; set; }
        public IValueSetValidator Validator { get; set; }
    }
}
