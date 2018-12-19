
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace Examine.AzureSearch
{
    public class AzureSearchIndex : BaseIndexProvider, IDisposable
    {
        
        private readonly string _searchServiceName;
        private readonly string _apiKey;
        private bool? _exists;
        private ISearchIndexClient _indexer;
        private readonly Lazy<ISearchServiceClient> _client;
        private static readonly object ExistsLocker = new object();

        public AzureSearchIndex(
            string name,
            FieldDefinitionCollection fieldDefinitions,
            string searchServiceName, string apiKey, string analyzer,
            IValueSetValidator validator = null)
            : base(name.ToLowerInvariant(), //TODO: Need to 'clean' the name according to Azure Search rules
                fieldDefinitions, validator)
        {
            _searchServiceName = searchServiceName;
            _apiKey = apiKey;
            Analyzer = analyzer;
            
            _client = new Lazy<ISearchServiceClient>(CreateSearchServiceClient);
        }

        /// <summary>
        /// The name of the analyzer to use by default for fields
        /// </summary>
        public string Analyzer { get; }

        ///// <summary>
        ///// Returns IIndexCriteria object from the IndexSet, used to configure the indexer during initialization
        ///// </summary>
        ///// <param name="indexSet"></param>
        //public virtual IIndexCriteria CreateIndexerData(IndexSet indexSet)
        //{
        //    return indexSet.ToIndexCriteria();
        //}

        //public int GetDocumentCount() => Convert.ToInt32(GetIndexer().Documents.Count());

        //public int GetFieldCount()
        //{
        //    var index = _client.Value.Indexes.Get(IndexSetName);
        //    return index.Fields.Count;
        //}

        private ISearchServiceClient CreateSearchServiceClient()
        {
            var serviceClient = new SearchServiceClient(_searchServiceName, new SearchCredentials(_apiKey));
            return serviceClient;
        }

        //protected override void DeleteItem(string id, Action<KeyValuePair<string, string>> onComplete)
        //{
        //    var indexer = GetIndexer();

        //    //TODO: Check exception: https://docs.microsoft.com/en-us/azure/search/search-howto-dotnet-sdk
        //    var result = indexer.Documents.Index(IndexBatch.Delete(FormatFieldName(IndexNodeIdFieldName), new[] { id }));

        //    onComplete(new KeyValuePair<string, string>(IndexNodeIdFieldName, id));
        //}

        private ISearchIndexClient GetIndexClient()
        {
            return _indexer ?? (_indexer = _client.Value.Indexes.GetClient(Name));
        }

        private void EnsureIndex(bool forceOverwrite)
        {
            if (!forceOverwrite && _exists.HasValue && _exists.Value) return;

            var indexExists = IndexExists();
            if (indexExists && !forceOverwrite) return;

            if (indexExists)
            {
                _client.Value.Indexes.Delete(Name);
            }

            CreateNewIndex();
        }

        //protected override void IndexItem(string id, string type, IDictionary<string, string> values, Action onComplete)
        //{
        //    //TODO: Run this on a background thread

        //    var indexer = GetIndexer();

        //    var doc = new Document();
        //    foreach (var r in values)
        //    {
        //        doc[FormatFieldName(r.Key)] = r.Value;
        //    }

        //    //TODO: Check exception: https://docs.microsoft.com/en-us/azure/search/search-howto-dotnet-sdk
        //    //TODO: move this to a method which includes an event
        //    var result = indexer.Documents.Index(IndexBatch.Upload(new[] { doc }));

        //    onComplete();
        //}

        //protected override void IndexItems(string type, IEnumerable<IndexDocument> docs, Action<IEnumerable<IndexedNode>> batchComplete)
        //{
        //    //TODO: Run this on a background thread

        //    var indexer = GetIndexer();
        //    DeleteAllDocumentsOfType(indexer, type);

        //    //batches can only contain 1000 records
        //    foreach (var rowGroup in docs.InGroupsOf(1000))
        //    {
        //        var batch = IndexBatch.Upload(ToAzureSearchDocs(rowGroup));

        //        try
        //        {
        //            var indexResult = indexer.Documents.Index(batch);
        //            //TODO: Do we need to check for errors in any of the results?

        //            batchComplete(indexResult.Results.Select(x => new IndexedNode
        //            {
        //                NodeId = int.Parse(x.Key), //TODO: error check
        //                Type = type
        //            }));
        //        }
        //        catch (IndexBatchException e)
        //        {
        //            //TODO: Check exception: https://docs.microsoft.com/en-us/azure/search/search-howto-dotnet-sdk and retry

        //            // Sometimes when your Search service is under load, indexing will fail for some of the documents in
        //            // the batch. Depending on your application, you can take compensating actions like delaying and
        //            // retrying. For this simple demo, we just log the failed document keys and continue.

        //            //TODO: Output to abstract ILogger
        //            Console.WriteLine(
        //                "Failed to index some of the documents: {0}",
        //                string.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key)));
        //        }
        //    }
        //}

        //private static void DeleteAllDocumentsOfType(ISearchIndexClient indexer, string type)
        //{
        //    // Query all
        //    var searchResult = indexer.Documents.Search<Document>($"{FormatFieldName(IndexTypeFieldName)}:{type}");

        //    if (searchResult.Results.Count == 0)
        //        return;

        //    var toDelete =
        //        searchResult
        //            .Results
        //            .Select(r => r.Document["id"].ToString());

        //    // Delete all
        //    try
        //    {
        //        var batch = IndexBatch.Delete(FormatFieldName(IndexNodeIdFieldName), toDelete);
        //        var result = indexer.Documents.Index(batch);
        //    }
        //    catch (IndexBatchException ex)
        //    {
        //        //TODO: Check exception: https://docs.microsoft.com/en-us/azure/search/search-howto-dotnet-sdk and retry

        //        //TODO: Output to abstract ILogger
        //        Console.WriteLine($"Failed to delete documents: {string.Join(", ", ex.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key))}");
        //        throw;
        //    }
        //}

        private static IEnumerable<Document> ToAzureSearchDocs(IEnumerable<ValueSet> docs)
        {
            foreach (var d in docs)
            {
                //this is just a dictionary
                var ad = new Document
                {
                    [FormatFieldName(LuceneIndex.ItemIdFieldName)] = d.Id,
                    [FormatFieldName(LuceneIndex.ItemTypeFieldName)] = d.ItemType,
                    [FormatFieldName(LuceneIndex.CategoryFieldName)] = d.Category
                };
                foreach (var i in d.Values)
                {
                    if (i.Value.Count > 0)
                        ad[FormatFieldName(i.Key)] = i.Value[0]; //TODO: Need to find out if we can pass multiple values per field
                }
                yield return ad;
            }
        }

        private Field CreateField(FieldDefinition field)
        {
            var dataType = FromExamineType(field.Type);
            return new Field(FormatFieldName(field.Name), dataType)
            {
                IsSearchable = dataType == DataType.String,
                //TODO: We don't have an equivalent IIndexValueType thing for AzureSearch yet  so can't determine right now
                IsSortable = dataType != DataType.String,
                //TODO: We don't have an equivalent IIndexValueType thing for AzureSearch yet  so can't determine right now
                Analyzer = dataType == DataType.String ? FromLuceneAnalyzer(Analyzer) : null
            };
        }

        private void CreateNewIndex()
        {
            lock (ExistsLocker)
            {
                var fields = FieldDefinitionCollection.Select(CreateField).ToList();

                //id must be string
                fields.Add(new Field(FormatFieldName(LuceneIndex.ItemIdFieldName), DataType.String)
                {
                    IsKey = true,
                    IsSortable = true,
                    IsSearchable = true,
                    Analyzer = AnalyzerName.Whitespace
                });

                fields.Add(new Field(FormatFieldName(LuceneIndex.ItemTypeFieldName), DataType.String)
                {
                    IsSearchable = true,
                    Analyzer = AnalyzerName.Whitespace
                });

                //TODO: We should have a custom event for devs to modify the AzureSearch data directly here

                var index = _client.Value.Indexes.Create(new Index(Name, fields));
                _exists = true;
            }
        }

        private static AnalyzerName FromLuceneAnalyzer(string analyzer)
        {
            //not fully qualified, just return the type
            if (!analyzer.Contains(","))
                return AnalyzerName.Create(analyzer);

            //if it contains a comma, we'll assume it's an assembly typed name

            if (analyzer.Contains("StandardAnalyzer"))
                return AnalyzerName.StandardLucene;
            if (analyzer.Contains("WhitespaceAnalyzer"))
                return AnalyzerName.Whitespace;
            if (analyzer.Contains("SimpleAnalyzer"))
                return AnalyzerName.Simple;
            if (analyzer.Contains("KeywordAnalyzer"))
                return AnalyzerName.Keyword;
            if (analyzer.Contains("StopAnalyzer"))
                return AnalyzerName.Stop;

            if (analyzer.Contains("ArabicAnalyzer"))
                return AnalyzerName.ArLucene;
            if (analyzer.Contains("BrazilianAnalyzer"))
                return AnalyzerName.PtBRLucene;
            if (analyzer.Contains("ChineseAnalyzer"))
                return AnalyzerName.ZhHansLucene;
            //if (analyzer.Contains("CJKAnalyzer")) //TODO: Not sure where this maps
            //    return AnalyzerName.ZhHansLucene;
            if (analyzer.Contains("CzechAnalyzer"))
                return AnalyzerName.CsLucene;
            if (analyzer.Contains("DutchAnalyzer"))
                return AnalyzerName.NlLucene;
            if (analyzer.Contains("FrenchAnalyzer"))
                return AnalyzerName.FrLucene;
            if (analyzer.Contains("GermanAnalyzer"))
                return AnalyzerName.DeLucene;
            if (analyzer.Contains("RussianAnalyzer"))
                return AnalyzerName.RuLucene;

            //if the above fails, return standard
            return AnalyzerName.StandardLucene;

        }

        public static string FormatFieldName(string fieldName)
        {
            if (fieldName.StartsWith(LuceneIndex.SpecialFieldPrefix))
            {
                //azure search requires that it starts with a letter
                return $"z{fieldName}";
            }
            return fieldName;
        }

        private static DataType FromExamineType(string type)
        {
            switch (type.ToLowerInvariant())
            {
                case "date":
                case "datetimeoffset":
                    return DataType.DateTimeOffset;
                case "double":
                case "float":
                    return DataType.Double;
                case "long":
                    return DataType.Int64;
                case "int":
                case "number":
                    return DataType.Int32;
                default:
                    return DataType.String;
            }
        }


        protected override void PerformIndexItems(IEnumerable<ValueSet> op, Action<IndexOperationEventArgs> onComplete)
        {
            //TODO: Run this on a background thread

            var indexer = GetIndexClient();

            //batches can only contain 1000 records
            foreach (var rowGroup in op.InGroupsOf(1000))
            {
                var batch = IndexBatch.Upload(ToAzureSearchDocs(rowGroup));

                try
                {
                    var indexResult = indexer.Documents.Index(batch);
                    //TODO: Do we need to check for errors in any of the results?

                    onComplete(new IndexOperationEventArgs(this, indexResult.Results.Count));
                }
                catch (IndexBatchException e)
                {
                    //TODO: Check exception: https://docs.microsoft.com/en-us/azure/search/search-howto-dotnet-sdk and retry

                    // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                    // the batch. Depending on your application, you can take compensating actions like delaying and
                    // retrying. For this simple demo, we just log the failed document keys and continue.

                    //TODO: Output to abstract ILogger
                    Console.WriteLine(
                        "Failed to index some of the documents: {0}",
                        string.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key)));
                }
            }
        }

        protected override void PerformDeleteFromIndex(string itemId, Action<IndexOperationEventArgs> onComplete)
        {
            var indexer = GetIndexClient();

            //TODO: Check exception: https://docs.microsoft.com/en-us/azure/search/search-howto-dotnet-sdk
            var result = indexer.Documents.Index(IndexBatch.Delete(FormatFieldName(LuceneIndex.ItemIdFieldName), new[] { itemId }));
            
            onComplete(new IndexOperationEventArgs(this, result.Results.Count));
        }

        public override ISearcher GetSearcher()
        {
            throw new NotImplementedException();
        }

        public override void CreateIndex()
        {
            EnsureIndex(true);
        }

        public override bool IndexExists()
        {
            lock (ExistsLocker)
            {
                return _exists ?? (_exists = _client.Value.Indexes.Exists(Name)).Value;
            }
        }

        public void Dispose()
        {
            _indexer?.Dispose();
            if (_client.IsValueCreated)
                _client.Value.Dispose();
        }
    }
}
