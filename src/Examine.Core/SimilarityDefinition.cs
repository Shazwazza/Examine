using System;

namespace Examine
{
    /// <summary>
    /// Defines a Similarity for an Index
    /// </summary>
    public class SimilarityDefinition
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the similarity</param>
        /// <param name="type">Name of the similarity Type</param>
        public SimilarityDefinition(string name, string type)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            Name = name;
            Type = type;
        }

        /// <summary>
        /// The name of the Similarity
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// The name of Type of Similarity
        /// </summary>
        public string Type { get; }
    }
}
