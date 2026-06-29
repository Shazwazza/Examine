using System.Collections.Generic;
using Examine.Lucene.Indexing;

namespace Examine.Lucene.Search
{

    public class MultiSearchContext : ISearchContext
    {
        private readonly ISearchContext[] _inner;
        
        private string[] _fields;
        
        public MultiSearchContext(ISearchContext[] inner) => _inner = inner;

        public ISearcherReference GetSearcher()
        {
            var searchers = new ISearcherReference[_inner.Length];
            for (var i = 0; i < _inner.Length; i++)
                searchers[i] = _inner[i].GetSearcher();
            return new MultiSearchSearcherReference(searchers);
        }

        public string[] SearchableFields => _fields ?? (_fields = BuildSearchableFields());

        private string[] BuildSearchableFields()
        {
            var seen = new HashSet<string>();
            foreach (var ctx in _inner)
                foreach (var f in ctx.SearchableFields)
                    seen.Add(f);
            var result = new string[seen.Count];
            seen.CopyTo(result);
            return result;
        }

        public IIndexFieldValueType GetFieldValueType(string fieldName)
        {
            foreach (var ctx in _inner)
            {
                var type = ctx.GetFieldValueType(fieldName);
                if (type != null) return type;
            }
            return null;
        }

    }
}
