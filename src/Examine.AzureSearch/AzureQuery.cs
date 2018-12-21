using System;
using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Search;
using Examine.Search;
using Lucene.Net.Search;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Analyzer = Lucene.Net.Analysis.Analyzer;
using StandardAnalyzer = Lucene.Net.Analysis.Standard.StandardAnalyzer;

namespace Examine.AzureSearch
{
    public class AzureQuery : LuceneSearchQueryBase, IQuery, IQueryExecutor
    {
        internal Analyzer DefaultAnalyzer { get; } = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
        internal static readonly LuceneSearchOptions EmptyOptions = new LuceneSearchOptions();
        public ISearchIndexClient IndexClient { get; }
        public ISearchServiceClient ServiceClient { get; }

        public AzureQuery(AzureQuery previous, BooleanOperation op)
            : base(previous.Category, previous.DefaultAnalyzer, null, EmptyOptions, op)
        {
            IndexClient = previous.IndexClient;
            ServiceClient = previous.ServiceClient;
        }

        public AzureQuery(
            ISearchIndexClient indexClient, ISearchServiceClient serviceClient,
            string category, string[] fields, BooleanOperation op)
        : base(category, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29), fields, EmptyOptions, op)
        {
            IndexClient = indexClient;
            ServiceClient = serviceClient;
        }

        protected override LuceneBooleanOperationBase CreateOp()
        {
            return new AzureBooleanOperation(this);
        }

        public override IBooleanOperation Field<T>(string fieldName, T fieldValue) 
        {
            throw new NotImplementedException();
        }

        public override IBooleanOperation ManagedQuery(string query, string[] fields = null)
        {
            //TODO: Instead of AllFields here we should have a reference to the FieldDefinitionCollection
            foreach (var field in fields ?? AllFields)
            {
                var fullTextQuery = FullTextType.GenerateQuery(field, query, DefaultAnalyzer);
                Query.Add(fullTextQuery, Occurrence);
            }
            return new AzureBooleanOperation(this);
        }

        public override IBooleanOperation RangeQuery<T>(string[] fields, T? min, T? max, bool minInclusive = true, bool maxInclusive = true) 
        {
            throw new NotImplementedException();
        }

        public ISearchResults Execute(int maxResults = 500)
        {
            return new AzureSearchResults(IndexClient.Documents, Query, maxResults);
        }
        
    }
}