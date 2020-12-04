using Azure.Storage.Blobs;
using Examine.AzureDirectory;
using Examine.Test;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Text;

namespace Examine.Test.AzureDirectoryTests
{
    [TestFixture]
    public class IntegrationTests
    {
        private ILogger _logger;
        private ILoggerFactory _loggerProvider;
        [SetUp]
        public void Setup()
        {
            _loggerProvider = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = _loggerProvider.CreateLogger<IntegrationTests>();
        }
        [TearDown]
        public void Teardown()
        {
            _loggerProvider?.Dispose();
        }

        [Explicit("Requires storage emulator to be running")]
        [Test]
        public void TestReadAndWrite()
        {
            var connectionString = Environment.GetEnvironmentVariable("DataConnectionString") ?? "UseDevelopmentStorage=true";

            var cloudStorageAccount = connectionString;
            string containerName = "testcatalog";
            // default AzureDirectory stores cache in local temp folder
            using (var cacheDirectory = new RandomIdRAMDirectory())
            {
                var azureDirectory = new AzureLuceneDirectory(_logger, cloudStorageAccount, containerName, cacheDirectory);

                using (var indexWriter = new IndexWriter(azureDirectory, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT), !IndexReader.IndexExists(azureDirectory), new Lucene.Net.Index.IndexWriter.MaxFieldLength(IndexWriter.DEFAULT_MAX_FIELD_LENGTH)))
                {
                    indexWriter.SetRAMBufferSizeMB(10.0);

                    for (int iDoc = 0; iDoc < 10000; iDoc++)
                    {
                        var doc = new Document();
                        doc.Add(new Field("id", DateTime.Now.ToFileTimeUtc().ToString() + "-" + iDoc.ToString(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.NO));
                        doc.Add(new Field("Title", GeneratePhrase(10), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.NO));
                        doc.Add(new Field("Body", GeneratePhrase(40), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.NO));
                        indexWriter.AddDocument(doc);
                    }

                    Console.WriteLine("Total docs is {0}", indexWriter.NumDocs());
                }
                for (var i = 0; i < 100; i++)
                {
                    using (var searcher = new IndexSearcher(azureDirectory))
                    {
                        Assert.AreNotEqual(0, SearchForPhrase(searcher, "dog"));
                        Assert.AreNotEqual(0, SearchForPhrase(searcher, "cat"));
                        Assert.AreNotEqual(0, SearchForPhrase(searcher, "car"));
                    }
                }

                // check the container exists, and delete it
                var containerClient = new BlobContainerClient(cloudStorageAccount, containerName);
                var exists = containerClient.Exists();
                Assert.IsTrue(exists); // check the container exists
                containerClient.Delete();
            }
        }


        static int SearchForPhrase(IndexSearcher searcher, string phrase)
        {
            var parser = new Lucene.Net.QueryParsers.QueryParser(Lucene.Net.Util.Version.LUCENE_CURRENT, "Body", new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT));
            var query = parser.Parse(phrase);
            return searcher.Search(query, 100).TotalHits;
        }

        static Random _random = new Random();
        static string[] sampleTerms =
            {
                "dog","cat","car","horse","door","tree","chair","microsoft","apple","adobe","google","golf","linux","windows","firefox","mouse","hornet","monkey","giraffe","computer","monitor",
                "steve","fred","lili","albert","tom","shane","gerald","chris",
                "love","hate","scared","fast","slow","new","old"
            };

        private static string GeneratePhrase(int MaxTerms)
        {
            var phrase = new StringBuilder();
            int nWords = 2 + _random.Next(MaxTerms);
            for (int i = 0; i < nWords; i++)
            {
                phrase.AppendFormat(" {0} {1}", sampleTerms[_random.Next(sampleTerms.Length)], _random.Next(32768).ToString());
            }
            return phrase.ToString();
        }
        protected System.IO.DirectoryInfo GetLocalStorageDirectory(System.IO.DirectoryInfo indexPath)
        {
            var appDomainHash = Guid.NewGuid().ToString().Replace("-", "");
            var indexPathName = GetIndexPathName(indexPath);
            var cachePath = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "ExamineIndexes",
                //include the appdomain hash is just a safety check, for example if a website is moved from worker A to worker B and then back
                // to worker A again, in theory the %temp%  folder should already be empty but we really want to make sure that its not
                // utilizing an old index
                appDomainHash, indexPathName);
            var azureDir = new System.IO.DirectoryInfo(cachePath);
            if (azureDir.Exists == false)
                azureDir.Create();
            return azureDir;
        } 
        /// <summary>
          /// Return a sub folder name to store under the temp folder
          /// </summary>
          /// <param name="indexPath"></param>
          /// <returns>
          /// A hash value of the original path
          /// </returns>
        private static string GetIndexPathName(System.IO.DirectoryInfo indexPath)
        {
            return indexPath.FullName.GenerateHash();
        }
    }
}