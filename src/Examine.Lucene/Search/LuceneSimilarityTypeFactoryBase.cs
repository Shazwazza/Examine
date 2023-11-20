using Examine.Lucene.Indexing;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Base Class for Lucene.NET Similarity Defintions
    /// </summary>
    public abstract class LuceneSimilarityTypeFactoryBase
        : ISimilarityTypeFactory
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Similarity Name</param>
        public LuceneSimilarityTypeFactoryBase(string name) 
        {
            Name = name;
        }

        /// <summary>
        /// Similarity Name
        /// </summary>
        public string Name { get; }

        /// <inheritdoc/>
        public abstract IIndexSimilarityType Create();

    }
}
