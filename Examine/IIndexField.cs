namespace Examine
{
    /// <summary>
    /// Represents a field to index
    /// </summary>
    public interface IIndexField
    {
        /// <summary>
        /// The name of the index field
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// Whether or not this field has sorting enabled in search results
        /// </summary>
        bool EnableSorting { get; set; }

        /// <summary>
        /// The data type
        /// </summary>
        string Type { get; set; }
    }
}