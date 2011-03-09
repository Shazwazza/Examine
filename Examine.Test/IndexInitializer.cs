using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UmbracoExamine.Config;
using UmbracoExamine;
using Examine.Test.DataServices;
using System.Threading;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis;
using Examine.Providers;
using System.Collections.Specialized;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Search;

namespace Examine.Test
{
    /// <summary>
    /// Used internally by test classes to initialize a new index from the template
    /// </summary>
    internal static class IndexInitializer
    {
        public static void Initialize()
        {

            if (_initIndexFolders == null)
            {
                _initIndexFolders = new Dictionary<string, string>();
                
                //wire up error handling
                foreach (var p in ExamineManager.Instance.IndexProviderCollection.Cast<BaseIndexProvider>())
                {
                    p.IndexingError += IndexingError;
                }
            }

            //RemoveWorkingIndex();

            CreateIndexesFromTemplate();
            ResetSearchers();
            ResetIndexers();

           
        }

        /// <summary>
        /// Stores reference to the initial index folders for each provider
        /// </summary>
        private static Dictionary<string, string> _initIndexFolders;


        static void IndexingError(object sender, IndexingErrorEventArgs e)
        {
            throw new ApplicationException(e.Message, e.InnerException);
        }

        /// <summary>
        /// return the folder containing the index template
        /// </summary>
        /// <returns></returns>
        private static DirectoryInfo GetTemplateFolder() {
            var template = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory
                .GetDirectories("App_Data")
                .Single()
                .GetDirectories("TemplateIndex")
                .Single();
            return template;
        }

        private static void ResetSearchers()
        {
            foreach(var s in ExamineManager.Instance.SearchProviderCollection.Cast<ISearcher>().OfType<LuceneSearcher>())
            {
                //reset the folder 
                s.LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[s.IndexSetName].IndexPath, "Index"));
            }            
        }

        private static void ResetIndexers()
        {
            foreach (var i in ExamineManager.Instance.IndexProviderCollection.OfType<LuceneIndexer>())
            {
                i.LuceneIndexFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[i.IndexSetName].IndexPath, "Index"));
                i.IndexQueueItemFolder = new DirectoryInfo(Path.Combine(IndexSets.Instance.Sets[i.IndexSetName].IndexPath, "Queue"));
            }
        }

        /// <summary>
        /// This creates indexes for each index set based on the template
        /// </summary>
        private static void CreateIndexesFromTemplate()
        {
            var template = GetTemplateFolder();
            IndexSets.Instance.Sets.Cast<IndexSet>().ToList()
                .ForEach(x =>
                {

                    DirectoryInfo di;
                    //create a new random path
                    if (_initIndexFolders.ContainsKey(x.SetName))
                    {
                        di = new DirectoryInfo(Path.Combine(_initIndexFolders[x.SetName], Guid.NewGuid().ToString()));
                    }
                    else
                    {                        
                        di = new DirectoryInfo(Path.Combine(x.IndexPath, Guid.NewGuid().ToString()));    
                        //add to the dictionary
                        _initIndexFolders.Add(x.SetName, x.IndexPath);
                    }

                    if (!di.Exists)
                    {
                        di.Create();
                    }

                    //set this path back to the provider
                    x.IndexPath = di.FullName;

                    var indexDir = di.CreateSubdirectory("Index");
                    template.GetFiles().ToList().ForEach(f => f.CopyTo(Path.Combine(indexDir.FullName, f.Name)));
                });
        }

    }
}
