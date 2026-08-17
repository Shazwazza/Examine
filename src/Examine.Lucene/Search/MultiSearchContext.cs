using System.Collections.Generic;
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
        {
            var searchers = new ISearcherReference[_inner.Length];
            for (var i = 0; i < _inner.Length; i++)
            {
                searchers[i] = _inner[i].GetSearcher();
            }

            return new MultiSearchSearcherReference(searchers);
        }

        /// <inheritdoc/>
        public string[] SearchableFields => _fields ??= BuildSearchableFields();

        // Manual loops replace SelectMany().Distinct().ToArray() to avoid the LINQ
        // iterator allocations on each rebuild.
        private string[] BuildSearchableFields()
        {
            var seen = new HashSet<string>();
            foreach (var ctx in _inner)
            {
                foreach (var f in ctx.SearchableFields)
                {
                    seen.Add(f);
                }
            }

            var result = new string[seen.Count];
            seen.CopyTo(result);
            return result;
        }

        /// <inheritdoc/>
        public IIndexFieldValueType? GetFieldValueType(string fieldName)
        {
            foreach (var ctx in _inner)
            {
                var type = ctx.GetFieldValueType(fieldName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

    }
}
