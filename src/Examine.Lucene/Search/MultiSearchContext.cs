using System.Collections.Generic;
using System.Linq;
using Examine.Lucene.Indexing;

namespace Examine.Lucene.Search
{

    public class MultiSearchContext : ISearchContext
    {
        private readonly ISearchContext[] _inner;

        private string[] _fields;

        public MultiSearchContext(ISearchContext[] inner) => _inner = inner;

        public ISearcherReference GetSearcher()
            => new MultiSearchSearcherReference(_inner.Select(x => x.GetSearcher()).ToArray());

        public string[] SearchableFields => _fields ?? (_fields = _inner.SelectMany(x => x.SearchableFields).Distinct().ToArray());

        public IIndexFieldValueType GetFieldValueType(string fieldName)
            => _inner.Select(cc => cc.GetFieldValueType(fieldName)).FirstOrDefault(type => type != null);

        public RelevanceScorerDefinition GetRelevanceScorer(string scorerName) => throw new System.NotImplementedException();
    }
}
