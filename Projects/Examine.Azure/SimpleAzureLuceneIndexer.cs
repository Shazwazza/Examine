using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Examine.Azure
{
    public class SimpleAzureLuceneIndexer : SimpleDataIndexer, IAzureCatalogue
    {
        /// <summary>
        /// static constructor run to initialize azure settings
        /// </summary>
        static SimpleAzureLuceneIndexer()
        {
            AzureExtensions.EnsureAzureConfig();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SimpleAzureLuceneIndexer()
        {
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="workingFolder"></param>
        /// <param name="analyzer"></param>
        /// <param name="dataService"></param>
        /// <param name="indexTypes"></param>
        /// <param name="async"></param>
        public SimpleAzureLuceneIndexer(IIndexCriteria indexerData, DirectoryInfo workingFolder, Analyzer analyzer, ISimpleDataService dataService, IEnumerable<string> indexTypes, bool async)
            : base(indexerData, workingFolder, analyzer, dataService, indexTypes, async)
        {
        }

        /// <summary>
        /// The blob storage catalogue name to store the index in
        /// </summary>
        public string Catalogue { get; private set; }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            this.SetOptimizationThresholdOnInit(config);
            var indexSet = IndexSets.Instance.Sets[IndexSetName];
            Catalogue = indexSet.IndexPath;
        }

        private Lucene.Net.Store.Directory _directory;
		
		[SecuritySafeCritical]
        public override Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            return _directory ?? (_directory = this.GetAzureDirectory());
        }

        //[SecuritySafeCritical]
        //public override Lucene.Net.Index.IndexWriter GetIndexWriter()
        //{
        //    return this.GetAzureIndexWriter();
        //}
    }
}