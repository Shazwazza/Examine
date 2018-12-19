using System;
using System.Linq;
using Examine.Providers;
using Examine.Search;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using StandardAnalyzer = Lucene.Net.Analysis.Standard.StandardAnalyzer;

namespace Examine.AzureSearch
{
    public class AzureSearchSearcher : BaseSearchProvider, IDisposable
    {
        private readonly string _searchServiceName;
        private readonly string _apiKey;
        private readonly Lazy<ISearchIndexClient> _indexClient;
        private readonly Lazy<ISearchServiceClient> _serviceClient;
        private string[] _allFields;
        private bool? _exists;
        private static readonly string[] EmptyFields = new string[0];

        /// <summary>
        /// Constructor used for runtime based instances
        /// </summary>
        /// <param name="name"></param>
        /// <param name="searchServiceName"></param>
        /// <param name="apiKey"></param>
        public AzureSearchSearcher(string name, string searchServiceName, string apiKey)
            : base(name.ToLowerInvariant())//TODO: Need to 'clean' the name according to Azure Search rules
        {
            _searchServiceName = searchServiceName;
            _apiKey = apiKey;
            _indexClient = new Lazy<ISearchIndexClient>(CreateSearchIndexClient);
            _serviceClient = new Lazy<ISearchServiceClient>(CreateSearchServiceClient);
        }

        public bool IndexExists
        {
            get
            {
                if (_exists == null || !_exists.Value)
                {
                    _exists = _serviceClient.Value.Indexes.Exists(Name);
                }
                return _exists.Value;
            }
        }

        public string[] AllFields
        {
            get
            {
                if (_allFields != null)
                    return _allFields;

                if (!IndexExists) return EmptyFields;

                var index = _serviceClient.Value.Indexes.Get(Name);
                _allFields = index.Fields.Select(x => x.Name).ToArray();
                return _allFields;
            }
        }

        //public override ISearchResults Search(string searchText, bool useWildcards)
        //{
        //    if (!IndexExists)
        //        return EmptySearchResults.Instance;

        //    if (!useWildcards)
        //    {
        //        //just do a simple azure search
        //        return new AzureSearchResults(_searchClient.Value.Documents, searchText);
        //    }

        //    var sc = CreateSearchCriteria();
        //    return TextSearchAllFields(searchText, true, sc);
        //}

        //public override ISearchResults Search(string searchText, bool useWildcards, string indexType)
        //{
        //    if (!IndexExists)
        //        return EmptySearchResults.Instance;

        //    var sc = CreateSearchCriteria(indexType);
        //    return TextSearchAllFields(searchText, useWildcards, sc);
        //}

        //public override ISearchResults Search(ISearchCriteria searchParams, int maxResults)
        //{
        //    if (searchParams == null) throw new ArgumentNullException(nameof(searchParams));

        //    if (!IndexExists)
        //        return EmptySearchResults.Instance;

        //    var luceneParams = searchParams as LuceneSearchCriteria;
        //    if (luceneParams == null)
        //        throw new ArgumentException("Provided ISearchCriteria dos not match the allowed ISearchCriteria. Ensure you only use an ISearchCriteria created from the current SearcherProvider");

        //    var pagesResults = new AzureSearchResults(_searchClient.Value.Documents, luceneParams.Query, maxResults == 0 ? null : (int?)maxResults);
        //    return pagesResults;
        //}

        //public override ISearchResults Search(ISearchCriteria searchParams)
        //{
        //    return Search(searchParams, 0);
        //}

        //public override ISearchCriteria CreateSearchCriteria()
        //{
        //    return CreateSearchCriteria(string.Empty, BooleanOperation.And);
        //}

        //public override ISearchCriteria CreateSearchCriteria(BooleanOperation defaultOperation)
        //{
        //    return CreateSearchCriteria(string.Empty, defaultOperation);
        //}

        //public override ISearchCriteria CreateSearchCriteria(string type, BooleanOperation defaultOperation)
        //{
        //    return new LuceneSearchCriteria(type, _standardAnalyzer, AllFields, true, defaultOperation);
        //}

        //public override ISearchCriteria CreateSearchCriteria(string type)
        //{
        //    return CreateSearchCriteria(type, BooleanOperation.And);
        //}

        //private ISearchResults TextSearchAllFields(string searchText, bool useWildcards, ISearchCriteria sc)
        //{
        //    var splitSearch = searchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        //    if (useWildcards)
        //    {
        //        sc = sc.GroupedOr(AllFields,
        //            splitSearch.Select(x =>
        //                new ExamineValue(Examineness.ComplexWildcard, x.MultipleCharacterWildcard().Value)).Cast<IExamineValue>().ToArray()
        //        ).Compile();
        //    }
        //    else
        //    {
        //        sc = sc.GroupedOr(AllFields, splitSearch).Compile();
        //    }

        //    return Search(sc);
        //}

        private ISearchIndexClient CreateSearchIndexClient()
        {
            return new SearchIndexClient(_searchServiceName, Name, new SearchCredentials(_apiKey));
        }

        private ISearchServiceClient CreateSearchServiceClient()
        {
            return new SearchServiceClient(_searchServiceName, new SearchCredentials(_apiKey));
        }

        public void Dispose()
        {
            if (_indexClient.IsValueCreated)
                _indexClient.Value.Dispose();
            if (_serviceClient.IsValueCreated)
                _serviceClient.Value.Dispose();
        }

        public override ISearchResults Search(string searchText, int maxResults = 500)
        {
            //just do a simple azure search
            return new AzureSearchResults(_indexClient.Value.Documents, searchText, maxResults);
        }

        public override IQuery CreateQuery(string category = null, BooleanOperation defaultOperation = BooleanOperation.And)
        {
            return new AzureQuery(_indexClient.Value, _serviceClient.Value, category, AllFields, defaultOperation);
        }

        //public override ISearchResults Search(ISearchCriteria searchParameters, int maxResults = 500)
        //{
        //    if (searchParameters == null) throw new ArgumentNullException(nameof(searchParameters));

        //    if (!IndexExists)
        //        return EmptySearchResults.Instance;

        //    if (!(searchParameters is LuceneSearchCriteria luceneParams))
        //        throw new ArgumentException("Provided ISearchCriteria was not created with the CreateCriteria method of this searcher");

        //    var pagesResults = new AzureSearchResults(_searchClient.Value.Documents, luceneParams.Query, maxResults == 0 ? null : (int?)maxResults);
        //    return pagesResults;
        //}

        //public override ISearchCriteria CreateCriteria()
        //{
        //    throw new NotImplementedException();
        //}

        //public override ISearchCriteria CreateCriteria(BooleanOperation defaultOperation)
        //{
        //    throw new NotImplementedException();
        //}

        //public override ISearchCriteria CreateCriteria(string type, BooleanOperation defaultOperation)
        //{
        //    return new LuceneSearchCriteria(type, _standardAnalyzer, AllFields, true, defaultOperation);
        //}

        //public override ISearchCriteria CreateCriteria(string type)
        //{
        //    throw new NotImplementedException();
        //}
    }
}