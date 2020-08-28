using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.AzureDirectory;
using Lucene.Net.Analysis;
using System.Configuration;
using Microsoft.Azure.Storage;
using Lucene.Net.Store;
using System.IO;
using Lucene.Net.Index;
using Examine.Test.DataServices;
using System.Xml.Linq;
using UmbracoExamine;

namespace Examine.Test.AzureDirectory
{
    [TestFixture]
    public class ReadOnlyTest
    {
        private const string ContainerName = "examine-azure-directory-test";
        private readonly TestContentService _contentService = new TestContentService();

        private XElement GetRandomNode()
        {   
            //get a node from the data repo
            var nodes = _contentService.GetPublishedContentByXPath("//*[string-length(@id)>0 and number(@id)>0]")
                .Root
                .Elements()
                .ToList();
            var rand = new Random();
            return nodes[rand.Next(0, nodes.Count - 1)];
        }

        [Test]
        public void Read_Only_Sync()
        {
            var temp1 = Guid.NewGuid().ToString();
            var temp2 = Guid.NewGuid().ToString();
            var tempFolder1 = Path.Combine(TestContext.CurrentContext.WorkDirectory, temp1);
            var tempFolder2 = Path.Combine(TestContext.CurrentContext.WorkDirectory, temp2);
            System.IO.Directory.CreateDirectory(tempFolder1);
            System.IO.Directory.CreateDirectory(tempFolder2);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");

            SimpleFSDirectory cacheDirectory1 = new SimpleFSDirectory(new DirectoryInfo(tempFolder1));
            var writeDir = new Examine.AzureDirectory.AzureDirectory(
                storageAccount,
                ContainerName,
                cacheDirectory1,
                rootFolder: temp1,
                isReadOnly: false);

            SimpleFSDirectory cacheDirectory2 = new SimpleFSDirectory(new DirectoryInfo(tempFolder2));
            var readDir = new Examine.AzureDirectory.AzureDirectory(
                storageAccount,
                ContainerName,
                cacheDirectory2,
                rootFolder: temp1,
                isReadOnly: true);

            using (writeDir)
            using (readDir)
            using (var writeIndex = IndexInitializer.GetUmbracoIndexer(writeDir))
            using (var readIndex = IndexInitializer.GetUmbracoIndexer(readDir))
            using (var readSearcher = IndexInitializer.GetUmbracoSearcher(readDir))
            {
                writeIndex.EnsureIndex(true);
                readIndex.EnsureIndex(true);

                var tasks = new[]
                {
                    //new Task(() => writeIndex.RebuildIndex()),
                    new Task(() => 
                    {
                        var cloned = new XElement(GetRandomNode());
                        readIndex.ReIndexNode(cloned, IndexTypes.Content); 
                    }),
                    new Task(() =>
                    {
                        var cloned = new XElement(GetRandomNode());
                        readIndex.ReIndexNode(cloned, IndexTypes.Content);
                    })
                };
                foreach (var t in tasks) t.Start();
                Task.WaitAll(tasks);

                // force the reader to sync
                var search = readSearcher.Search("test", true);

                // verify that all files in the readonly cache dir have been synced from master blob storage
                var blobFiles = writeDir.GetAllBlobFiles();
                var readonlyCacheFiles = cacheDirectory2.GetDirectory().GetFiles("*.*").Select(x => x.Name).ToArray();
                AssertSyncedFiles(blobFiles, readonlyCacheFiles);
            }            
        }

        private void AssertSyncedFiles(string[] blobFiles, string[] cacheFiles)
        {
            var sortedBlobFiles = blobFiles.Where(x => !x.EndsWith(".lock")).Select(x => x.ToLowerInvariant()).OrderBy(x => x).ToList();
            var sortedCacheFiles = cacheFiles.Select(x => x.ToLowerInvariant()).OrderBy(x => x).ToList();

            CollectionAssert.AreEqual(sortedBlobFiles, sortedCacheFiles);
        }
    }
}
