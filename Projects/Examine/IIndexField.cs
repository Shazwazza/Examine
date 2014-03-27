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
        /// IndexName is so that you can index the same field with different analyzers
        /// </summary>
        /// <remarks>
        /// You might for instance both use a prefix indexer and a full text indexer. Also, if you have multiple data sources you can use a common field name in the index. If it's not specified it will just be the field name.
        /// 
        /// If this is null it should default to 'Name'
        /// </remarks>
        string IndexName { get; set; }        
        
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