using System;
using System.Configuration;

namespace Examine.AzureSearch
{
    public class AzureSearchConfig
    {
        public string ApiKey { get; }
        public string SearchServiceName { get; }

        public static AzureSearchConfig GetConfig(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(indexName));
            return new AzureSearchConfig(
                ConfigurationManager.AppSettings[$"examine:AzureSearch[{indexName}].apiKey"],
                ConfigurationManager.AppSettings[$"examine:AzureSearch[{indexName}].serviceName"]);
        }

        public AzureSearchConfig(string apiKey, string searchServiceName)
        {
            if (string.IsNullOrWhiteSpace(apiKey)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(apiKey));
            if (string.IsNullOrWhiteSpace(searchServiceName)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(searchServiceName));
            ApiKey = apiKey;
            SearchServiceName = searchServiceName;
        }
    }
}