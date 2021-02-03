using NUnit.Framework;
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
using System.Threading;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using System;
using Version = Lucene.Net.Util.Version;
using Examine.LuceneEngine.MergePolicies;
using Examine.LuceneEngine.MergeShedulers;
using Examine.LuceneEngine.DeletePolicies;
using Examine.RemoteDirectory;

namespace Examine.Test.AzureDirectory
{
    [TestFixture]
    public class ReadOnlyTest
    {
        private const string ContainerName = "examine-azure-syncDirectory-test";
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


        [Explicit("Explicit because the azure storage emulator needs to be running")]
        [Test]
        public void Read_Only_Sync()
        {
            var temp1 = Guid.NewGuid().ToString();
            var temp2 = Guid.NewGuid().ToString();
            var tempFolder1 = Path.Combine(TestContext.CurrentContext.WorkDirectory, temp1);
            var tempFolder2 = Path.Combine(TestContext.CurrentContext.WorkDirectory, temp2);
            System.IO.Directory.CreateDirectory(tempFolder1);
            System.IO.Directory.CreateDirectory(tempFolder2);

            var storageAccount = "UseDevelopmentStorage=true";
            var directory = new DirectoryInfo(tempFolder1);
            var cacheDirectory1 = new SimpleFSDirectory(directory);
            var remoteDirectory = new AzureRemoteDirectory(storageAccount, ContainerName,directory.Name);
            var writeDir = new Examine.RemoteDirectory.RemoteSyncDirectory(remoteDirectory,
                cacheDirectory1);

            writeDir.SetMergePolicyAction(e => new NoMergePolicy(e));
            writeDir.SetMergeScheduler(new NoMergeSheduler());
            writeDir.SetDeletion(new NoDeletionPolicy());
            var dir2 = new DirectoryInfo(tempFolder2);
            var readDir = new RemoteReadOnlyLuceneSyncDirectory(remoteDirectory, dir2.FullName, "test" );

            readDir.SetMergePolicyAction(e => new NoMergePolicy(e));
            readDir.SetMergeScheduler(new NoMergeSheduler());
            readDir.SetDeletion(new NoDeletionPolicy());
            using (writeDir)
            using (readDir)

            using (var luceneDir = new RandomIdRAMDirectory())
            using (var writeIndex = new TestIndex(writeDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var readIndex = new TestIndex(readDir, new StandardAnalyzer(Version.LUCENE_30)))
            using (var readSearcher = new TestIndex(readDir, new StandardAnalyzer(Version.LUCENE_30)))
            {

                writeIndex.EnsureIndex(true);
                readIndex.EnsureIndex(true);

                // Try to read/write with in parallel

                var tasks = new[]
                {
                    new Task(() => writeIndex.EnsureIndex(false)),              // write index
                    new Task(() =>
                    {
                        ValueSet cloned = GetRandomValueSet();
                        readIndex.IndexItem(cloned);  // readonly index doesn't write
                    }),
                    new Task(() =>
                    {
                       ValueSet cloned = GetRandomValueSet();
                        readIndex.IndexItem(cloned);  // readonly index doesn't write
                    }),
                    new Task(() =>
                    {
                        for(var a = 0; a < 3; a++)                          // write index x 3
                        {
                            ValueSet cloned = GetRandomValueSet();
                            writeIndex.IndexItem(cloned);
                        }
                    }),
                    new Task(() =>
                    {
                        for(var i = 0; i < 10; i++)
                        {
                            var s = readSearcher.GetSearcher().CreateQuery().NativeQuery("isTest:1");      // force the reader to sync
                            var sr = s.Execute();
                            var er = sr.Skip(0).ToList();
                            Thread.Sleep(50);
                        }
                    }),
                    new Task(() =>
                    {
                        ValueSet cloned = GetRandomValueSet();
                        readIndex.IndexItem(cloned);  // readonly index doesn't write //don't do this
                    })
                };
                foreach (var t in tasks) t.Start();
                Task.WaitAll(tasks);

                // force the reader to sync
                AsssertInSync(writeDir, readDir, readSearcher, "isTest:1");
                writeDir.Sync("segments.gen");
                //Now update the index and check it syncs again
                ValueSet cloned1 = GetRandomValueSet();
                cloned1.Add("UpdatedSync", 1);
                writeIndex.IndexItem(cloned1);
                writeIndex.ProcessNonAsync();

                AsssertInSync(writeDir, readDir, readSearcher, "UpdatedSync:1");
            }
        }

        private void AsssertInSync(RemoteSyncDirectory writeDir, RemoteReadOnlyLuceneSyncDirectory readDir, TestIndex readSearcher,string query)
        {
            var search = readSearcher.GetSearcher().CreateQuery().NativeQuery(query);
            var searchResults = search.Execute();
            var executedResults = searchResults.Skip(0).ToList();
           // Assert.IsTrue(executedResults.Any());
            // verify that all files in the readonly cache dir have been synced from master blob storage
            var blobFiles = writeDir.GetAllBlobFiles();
            SimpleFSDirectory readCacheDir = readDir.CacheDirectory as SimpleFSDirectory;
            var readonlyCacheFiles = readCacheDir.Directory.GetFiles("*.*").Select(x => x.Name).ToArray();
            AssertSyncedFiles(blobFiles, readonlyCacheFiles);
        }

        private ValueSet GetRandomValueSet()
        {
            var x = new XElement(GetRandomNode());

            return ValueSet.FromObject(x.Attribute("id").Value, "content",
                 new { writerName = x.Attribute("writerName").Value, template = x.Attribute("template").Value, lat = -6.1357, lng = 39.3621, isTest = 1 });
        }

        private void AssertSyncedFiles(string[] blobFiles, string[] cacheFiles)
        {
            var sortedBlobFiles = blobFiles.Where(x => !x.EndsWith(".lock")).Select(x => x.ToLowerInvariant()).OrderBy(x => x).ToList();
            var sortedCacheFiles = cacheFiles.Select(x => x.ToLowerInvariant()).OrderBy(x => x).ToList();

            // Assert that all files in blob storage exist in the cache storage (there might be extras in cache storage and that's ok)
            CollectionAssert.IsSubsetOf(sortedBlobFiles, sortedCacheFiles);
        }
    }
}
