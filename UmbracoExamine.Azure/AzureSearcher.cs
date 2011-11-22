using System;
using Examine.LuceneEngine.Config;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace UmbracoExamine.Azure
{
    public class AzureSearcher : UmbracoExamineSearcher
    {
        protected string Catalogue { get; private set; }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

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
    }
}