﻿using NUnit.Framework;
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

            var cacheDirectory1 = new SimpleFSDirectory(new DirectoryInfo(tempFolder1));
            var writeDir = new AzureLuceneDirectory(
                storageAccount,
                ContainerName,
                cacheDirectory1,
                rootFolder: temp1,
                isReadOnly: false);

            writeDir.SetMergePolicyAction(e => new NoMergePolicy(e));
            writeDir.SetMergeScheduler(new NoMergeSheduler());
            writeDir.SetDeletion(new NoDeletionPolicy());
            var cacheDirectory2 = new SimpleFSDirectory(new DirectoryInfo(tempFolder2));
            var readDir = new AzureReadOnlyLuceneDirectory(
                storageAccount,
                ContainerName,
                tempFolder2,
                ContainerName,
                rootFolder: temp1);

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
                // Cancel all writing for the reader index
                // This is important! 
                // TODO: It might mean we need to bake this into the bootstrapping of the readonly directory factory else devs
                // will need to do this themselves. This is a requirement because the index should never be written to in readonly mode
                // else there will be errors even though we are using Noop operations, see comment in AzureDirectory.FileLength at the bottom.
                readIndex.DocumentWriting += (sender, args) =>
                {
                    args.Cancel = true;
                };

                writeIndex.EnsureIndex(true);
                readIndex.EnsureIndex(true);

                // Try to read/write with in parallel

                var tasks = new[]
                {
                    new Task(() => writeIndex.EnsureIndex(false)),              // write index
                    new Task(() =>
                    {
                        ValueSet cloned = GetRandomValueSet();
                       // readIndex.IndexItem(cloned);  // readonly index doesn't write
                    }),
                    new Task(() =>
                    {
                       ValueSet cloned = GetRandomValueSet();
                       // readIndex.IndexItem(cloned);  // readonly index doesn't write
                    }),
                    new Task(() =>
                    {
                        for(var i = 0; i < 3; i++)                          // write index x 3
                        {
                            ValueSet cloned = GetRandomValueSet();
                            writeIndex.IndexItem(cloned);
                        }
                    }),
                    new Task(() =>
                    {
                        //for(var i = 0; i < 3; i++)
                        //{
                        //    var s = readSearcher.GetSearcher().CreateQuery().NativeQuery("test");      // force the reader to sync
                        //    Thread.Sleep(50);
                        //}
                    }),
                    new Task(() =>
                    {
                        ValueSet cloned = GetRandomValueSet();
                      //  readIndex.IndexItem(cloned);  // readonly index doesn't write
                    })
                };
                foreach (var t in tasks) t.Start();
                Task.WaitAll(tasks);

                // force the reader to sync
                var search = readSearcher.GetSearcher().CreateQuery().NativeQuery("test");
                var searchResults = search.Execute();
                var executedResults = searchResults.Skip(0).ToList();
                // verify that all files in the readonly cache dir have been synced from master blob storage
                var blobFiles = writeDir.GetAllBlobFiles();
                SimpleFSDirectory readCacheDir = readDir.CacheDirectory as SimpleFSDirectory;
                var readonlyCacheFiles = readCacheDir.Directory.GetFiles("*.*").Select(x => x.Name).ToArray();
                AssertSyncedFiles(blobFiles, readonlyCacheFiles);
            }
        }

        private ValueSet GetRandomValueSet()
        {
            var x = new XElement(GetRandomNode());

            return ValueSet.FromObject(x.Attribute("id").Value, "content",
                 new { writerName = x.Attribute("writerName").Value, template = x.Attribute("template").Value, lat = -6.1357, lng = 39.3621 });
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
