using System.Collections.Generic;
using System.Linq;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.SearchCriteria
{
    public class MultiCriteriaContext : ICriteriaContext
    {
        private readonly ICriteriaContext[] _inner;
        
        public MultiCriteriaContext(Searcher searcher, ICriteriaContext[] inner)
        {
            _inner = inner;
            Searcher = searcher;
        }

        public Searcher Searcher { get; }

        public IEnumerable<IIndexValueType> ValueTypes
        {
            get { return _inner.SelectMany(cc => cc.ValueTypes); }
        }

        public IIndexValueType GetValueType(string fieldName)
        {
            return _inner.Select(cc => cc.GetValueType(fieldName)).FirstOrDefault(type => type != null);
        }
    }

    public class CriteriaContext : ICriteriaContext
    {
        private readonly IndexFieldValueTypes _indexFieldValueTypes;

        public CriteriaContext(IndexFieldValueTypes indexFieldValueTypes, Searcher searcher)
        {
            _indexFieldValueTypes = indexFieldValueTypes;
            Searcher = searcher;
        }

        public Searcher Searcher { get; }

        public IEnumerable<IIndexValueType> ValueTypes => _indexFieldValueTypes.ValueTypes;

        public IIndexValueType GetValueType(string fieldName)
        {
            return _indexFieldValueTypes.GetValueType(fieldName);
        }
    }

    public interface ICriteriaContext
    {
        Searcher Searcher { get; }
        IEnumerable<IIndexValueType> ValueTypes { get; }
        IIndexValueType GetValueType(string fieldName);
    }
}