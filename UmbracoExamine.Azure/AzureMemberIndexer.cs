using System;
using System.IO;
using Examine;
using Examine.LuceneEngine.Config;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using UmbracoExamine.DataServices;

namespace UmbracoExamine.Azure
{
    public class AzureMemberIndexer : UmbracoMemberIndexer
    {
        /// <summary>
        /// static constructor run to initialize azure settings
        /// </summary>
        static AzureMemberIndexer()
        {
            // get settings from azure settings or app.config
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                try
                {
                    configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
                }
                catch (Exception)
                {
                    // for a console app, reading from App.config
                    configSetter(System.Configuration.ConfigurationManager.AppSettings[configName]);
                }
            });
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public AzureMemberIndexer()
            : base() { }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        public AzureMemberIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, dataService, analyzer, async)
        {

        }

        protected string Catalogue { get; private set; }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            if (config["autoOptimizeCommitThreshold"] == null)
            {
                //by default we need a higher threshold according to lucene azure docs
                OptimizationCommitThreshold = 1000;
            }
            else
            {
                int autoCommitThreshold;
                if (int.TryParse(config["autoOptimizeCommitThreshold"], out autoCommitThreshold))
                {
                    OptimizationCommitThreshold = autoCommitThreshold;
                }
                else
                {
                    throw new ParseException("Could not parse autoCommitThreshold value into an integer");
                }
            }           

            var indexSet = IndexSets.Instance.Sets[IndexSetName];
            Catalogue = indexSet.IndexPath;

        }

        protected override Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            var azureDirectory = new AzureDirectory(CloudStorageAccount.FromConfigurationSetting("blobStorage"), Catalogue);
            return azureDirectory;
        }

        protected override Lucene.Net.Index.IndexWriter GetIndexWriter()
        {
            var writer = base.GetIndexWriter();
            writer.SetRAMBufferSizeMB(10.0);
            writer.SetUseCompoundFile(false);
            writer.SetMaxMergeDocs(10000);
            writer.SetMergeFactor(100);
            return writer;
        }
    }
}