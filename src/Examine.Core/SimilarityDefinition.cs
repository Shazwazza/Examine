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
        public SimilarityDefinition(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            Name = name;
        }

        /// <summary>
        /// The name of the Similarity
        /// </summary>
        public string Name { get; }
    }
}
