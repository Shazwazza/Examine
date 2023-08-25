namespace Examine
{
    /// <summary>
    /// Represents the index options for a <see cref="IIndex"/>
    /// </summary>
    public class IndexOptions
    {
        /// <inheritdoc/>
        public IndexOptions()
        {
            FieldDefinitions = new FieldDefinitionCollection();
        }

        /// <summary>
        /// The field definitions for the <see cref="IIndex"/>
        /// </summary>
        public FieldDefinitionCollection FieldDefinitions { get; set; }

        /// <summary>
        /// The validator for the <see cref="IIndex"/>
        /// </summary>
        public IValueSetValidator? Validator { get; set; }
    }
}
