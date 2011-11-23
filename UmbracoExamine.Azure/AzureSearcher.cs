using System;
using System.IO;
using Examine.LuceneEngine.Config;
using Lucene.Net.Analysis;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace UmbracoExamine.Azure
{
    public class AzureSearcher : UmbracoExamineSearcher
    {
        /// <summary>
        /// static constructor run to initialize azure settings
        /// </summary>
        static AzureSearcher()
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
        public AzureSearcher()
		{
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexPath"></param>
        /// <param name="analyzer"></param>
        public AzureSearcher(DirectoryInfo indexPath, Analyzer analyzer)
            : base(indexPath, analyzer)
        {            
        }


        protected string Catalogue { get; private set; }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);           
            var indexSet = IndexSets.Instance.Sets[IndexSetName];
            Catalogue = indexSet.IndexPath;
        }

        protected override Lucene.Net.Store.Directory GetLuceneDirectory()
        {
            var azureDirectory = new AzureDirectory(CloudStorageAccount.FromConfigurationSetting("blobStorage"), Catalogue);
            return azureDirectory;
        }
    }
}