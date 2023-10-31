using System.Collections.Generic;
using System.Linq;
using Examine.Lucene.Indexing;

namespace Examine.Lucene.Search
{
    /// <summary>
    /// Represents a multi search context
    /// </summary>
    public class MultiSearchContext : ISearchContext
    {
        private readonly ISearchContext[] _inner;
        
        private string[]? _fields;

        /// <inheritdoc/>
        public MultiSearchContext(ISearchContext[] inner)
        {
            _inner = inner;
        }

        /// <inheritdoc/>
        public ISearcherReference GetSearcher()
            => new MultiSearchSearcherReference(_inner.Select(x => x.GetSearcher()).ToArray());

        /// <inheritdoc/>
        public string[] SearchableFields => _fields ??= _inner.SelectMany(x => x.SearchableFields).Distinct().ToArray();

        /// <inheritdoc/>
        public IIndexFieldValueType? GetFieldValueType(string fieldName)
            => _inner.Select(cc => cc.GetFieldValueType(fieldName)).FirstOrDefault(type => type != null);

        public SimilarityDefinition GetSimilarity(string similarityName) => _inner.Select(cc => cc.GetSimilarity(similarityName)).FirstOrDefault(similarityDefinition => similarityDefinition != null);
    }
}
