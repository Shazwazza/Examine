using System.Collections.Generic;

namespace Examine
{
    public interface IExamineManager
    {
        IReadOnlyDictionary<string, IIndexer> IndexProviders { get; }

        void AddIndexer(string name, IIndexer indexer);
        void AddSearcher(string name, ISearcher searcher);
        void DeleteFromIndexes(string nodeId);
        void DeleteFromIndexes(string nodeId, IEnumerable<IIndexer> providers);
        void Dispose();
        IIndexer GetIndexer(string indexerName);
        ISearcher GetSearcher(string indexerName);
        void IndexAll(string indexCategory);
        void IndexItems(ValueSet[] nodes);
        void IndexItems(ValueSet[] nodes, IEnumerable<IIndexer> providers);
        void RebuildIndexes();
    }
}