using System.Collections.Generic;
using Examine.Lucene.Indexing;

namespace Examine.Lucene.Search
{
    public interface ISearchContext
    {
        ISearcherReference GetSearcher();

        string[] SearchableFields { get; }
        IIndexFieldValueType GetFieldValueType(string fieldName);

        SimilarityDefinition GetSimilarity(string similarityName);
    }
}
