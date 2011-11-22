using System;
using Examine.LuceneEngine.Config;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace UmbracoExamine.Azure
{
    public class AzureMemberIndexer : UmbracoMemberIndexer
    {
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