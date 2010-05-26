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

namespace Examine.Test
{
    /// <summary>
    /// Used internally by test classes to initialize a new index from the template
    /// </summary>
    internal class IndexInitializer
    {
        public IndexInitializer()
        {            
            
            RemoveWorkingIndex();

            UpdateProviderPathsAndCreateIndexFromTemplate();
        }

        private DirectoryInfo GetTemplateFolder() {
            var template = GetWorkingFolder()
                .GetDirectories("App_Data")
                .Single()
                .GetDirectories("TemplateIndex")
                .Single();
            return template;
        }

        private DirectoryInfo GetWorkingFolder()
        {
            return new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
        }

        public void RemoveWorkingIndex() 
        {            
            var searchTest = GetWorkingFolder().GetDirectories("App_Data").First().GetDirectories("SearchWorkingTest").FirstOrDefault();
            RemoveIndex(searchTest, "CWSIndex");

            var indexText = GetWorkingFolder().GetDirectories("App_Data").First().GetDirectories("IndexWorkingTest").FirstOrDefault();
            RemoveIndex(indexText, "CWSSearch");
        }

        /// <summary>
        /// This craziness will ensure that a folder containing an index will be deleted and remove all existing locks created
        /// by other tests.
        /// </summary>
        /// <param name="di"></param>
        /// <param name="indexName"></param>
        private void RemoveIndex(DirectoryInfo di, string indexName)
        {
            if (di != null)
            {
                var cwsSearch = (LuceneExamineSearcher)ExamineManager.Instance.SearchProviderCollection[indexName];

                try
                {
                    var s = cwsSearch.GetSearcher();
                    var r = s.GetIndexReader();
                    s.Close();
                    r.Close();
                }
                catch (ApplicationException) { }
                catch (NoSuchDirectoryException) { }
                catch (DirectoryNotFoundException) { }

                SimpleFSDirectory searchFolder = new SimpleFSDirectory(di);
                if (IndexWriter.IsLocked(searchFolder))
                {
                    IndexWriter.Unlock(searchFolder);
                }

                var writer = new IndexWriter(new SimpleFSDirectory(di), new SimpleAnalyzer(), true, IndexWriter.MaxFieldLength.UNLIMITED);
                writer.Close();

                di.GetFiles().ToList().ForEach(x => x.Delete());
                di.Delete(true);
            }         
        }

        /// <summary>
        /// This will update our providers paths to be relavent for the machine the tests are being run on
        /// </summary>
        private void UpdateProviderPathsAndCreateIndexFromTemplate()
        {
            var template = GetTemplateFolder();
            ExamineLuceneIndexes.Instance.Sets.Cast<IndexSet>().ToList()
                .ForEach(x =>
                {
                    //updtae the index set paths
                    x.IndexPath = Path.Combine(GetWorkingFolder().FullName, x.IndexPath);
                    
                    var di = new DirectoryInfo(x.IndexPath);
                    if (!di.Exists)
                    {
                        di.Create();
                    }

                    var indexDir = di.CreateSubdirectory("Index");
                    template.GetFiles().ToList().ForEach(f => f.CopyTo(Path.Combine(indexDir.FullName, f.Name)));

                    //set the searcher that goes with this index set
                    var searcher = ExamineManager.Instance.SearchProviderCollection.Cast<LuceneExamineSearcher>()
                        .Where(s => s.IndexSetName == x.SetName)
                        .SingleOrDefault();
                    if (searcher != null)
                    {
                        searcher.LuceneIndexFolder = indexDir;
                    }
                    

                    //set the indexer that goes with this index set
                    var indexer = ExamineManager.Instance.IndexProviderCollection.Cast<LuceneExamineIndexer>()
                        .Where(s => s.IndexSetName == x.SetName)
                        .SingleOrDefault();
                    if (indexer != null)
                    {
                        indexer.LuceneIndexFolder = indexDir;
                    }
                });
        }

    }
}
