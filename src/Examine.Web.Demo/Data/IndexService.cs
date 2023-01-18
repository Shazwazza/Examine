using System.Diagnostics;
using Examine.Lucene.Providers;
using Examine.Search;
using Examine.Suggest;
using Examine.Web.Demo.Controllers;
using Examine.Web.Demo.Data.Models;
using Lucene.Net.Search;

namespace Examine.Web.Demo.Data
{
    public class IndexService
    {
        private readonly IExamineManager _examineManager;
        private readonly BogusDataService _bogusDataService;

        public IndexService(IExamineManager examineManager, BogusDataService bogusDataService)
        {
            _examineManager = examineManager;
            _bogusDataService = bogusDataService;
        }

        public void RebuildIndex(string indexName, int dataSize)
        {
            var index = GetIndex(indexName);

            index.CreateIndex();

            var data = _bogusDataService.GenerateData(dataSize);

            index.IndexItems(data);
        }

        public IndexInformation GetIndexInformation(string indexName)
        {
            var index = GetIndex(indexName);

            if (index is IIndexStats indexStats)
            {
                var fields = indexStats.GetFieldNames();
                var searchers = new List<string>();
                var suggesters = new List<string>();
                if (!string.IsNullOrWhiteSpace(index.Searcher?.Name))
                {
                    searchers.Add(index.Searcher?.Name);
                }
                if (!string.IsNullOrWhiteSpace(index.Suggester?.Name))
                {
                    suggesters.Add(index.Suggester?.Name);
                }
               
                return new IndexInformation(
                    indexStats.GetDocumentCount(),
                    fields.ToList(), searchers, suggesters);
            }
            else
            {
                throw new InvalidOperationException($"Failed to get index stats on {indexName}");
            }
        }

        public void AddToIndex(string indexName, int dataSize)
        {
            var index = GetIndex(indexName);

            var data = _bogusDataService.GenerateData(dataSize);

            index.IndexItems(data);
        }

        public IEnumerable<IIndex> GetAllIndexes()
        {
            return _examineManager.Indexes;
        }

        public ISearchResults SearchNativeQuery(string indexName, string query)
        {
            var index = GetIndex(indexName);

            var searcher = index.Searcher;
            var criteria = searcher.CreateQuery();
            return criteria.NativeQuery(query).Execute();
        }

        public ISearchResults SearchNativeQueryAcrossIndexes(string query)
        {
            if (!_examineManager.TryGetSearcher("MultiIndexSearcher", out var multiIndexSearcher))
            {
                throw new InvalidOperationException("Failed to get MultiIndexSearcher");
            }

            var searcher = multiIndexSearcher;
            var criteria = searcher.CreateQuery();
            return criteria.NativeQuery(query).Execute();
        }

        public ISearchResults GetAllIndexedItems(string indexName, int skip, int take)
        {
            var index = GetIndex(indexName);

            var searcher = index.Searcher;
            var criteria = searcher.CreateQuery();
            return criteria.All().Execute(QueryOptions.SkipTake(skip, take));
        }

        private IIndex GetIndex(string indexName)
        {
            if (!_examineManager.TryGetIndex(indexName, out var index))
            {
                throw new ArgumentException($"Index '{indexName}' not found");
            }

            return index;
        }

        public ISuggestionResults Suggest(string indexName, string query)
        {
            var index = GetIndex(indexName);

            var suggester = index.Suggester;
            var criteria = suggester.CreateSuggestionQuery();
            return criteria.SourceField("FullName").Execute(query, new SuggestionOptions(5, ExamineLuceneSuggesterNames.DirectSpellChecker));
        }
    }

}
