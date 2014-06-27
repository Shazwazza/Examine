using System;
using System.ComponentModel;

namespace Examine
{
    /// <summary>
    /// Represents a field to index
    /// </summary>
    [Obsolete("Use IFieldDefinition instead")]
    public interface IIndexField
    {
        /// <summary>
        /// The name of the index field
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// Whether or not this field has sorting enabled in search results
        /// </summary>
        [Obsolete("This is no longer used, sorting is enabled only by data type")]        
        bool EnableSorting { get; set; }
        
        /// <summary>
        /// The data type
        /// </summary>
        string Type { get; set; }
    }
}