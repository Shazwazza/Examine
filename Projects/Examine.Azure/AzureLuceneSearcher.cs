using System;
using System.IO;
using System.Security;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Examine.Azure
{
    public class AzureLuceneSearcher : LuceneSearcher, IAzureCatalogue
    {
        /// <summary>
        /// static constructor run to initialize azure settings
        /// </summary>
        static AzureLuceneSearcher()
        {
            AzureExtensions.EnsureAzureConfig();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AzureLuceneSearcher()
        {
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexPath"></param>
        /// <param name="analyzer"></param>
        public AzureLuceneSearcher(DirectoryInfo indexPath, Analyzer analyzer)
            : base(indexPath, analyzer)
        {            
        }


        public string Catalogue { get; private set; }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);           
            var indexSet = IndexSets.Instance.Sets[IndexSetName];
            Catalogue = indexSet.IndexPath;
        }

        private Lucene.Net.Store.Directory _directory;

		
        protected override Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            //always return one instance.
            return _directory ?? (_directory = this.GetAzureDirectory());
        }
    }
}