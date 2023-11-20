using Examine.Lucene.Indexing;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Base Class for Lucene.NET Similarity Defintions
    /// </summary>
    public abstract class LuceneSimilarityFactoryBase
        : ISimilarityFactory
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Similarity Name</param>
        public LuceneSimilarityFactoryBase(string name) 
        {
            Name = name;
        }

        /// <summary>
        /// Similarity Name
        /// </summary>
        public string Name { get; }

        public abstract IIndexSimilarity Create();

    }
}
