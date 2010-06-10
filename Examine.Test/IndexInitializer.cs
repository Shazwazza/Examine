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

            CreateIndexesFromTemplate();
        }

        /// <summary>
        /// return the folder containing the index template
        /// </summary>
        /// <returns></returns>
        private DirectoryInfo GetTemplateFolder() {
            var template = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory
                .GetDirectories("App_Data")
                .Single()
                .GetDirectories("TemplateIndex")
                .Single();
            return template;
        }

        /// <summary>
        /// Return the folder that we're going to run our test indexes in
        /// </summary>
        /// <returns></returns>
        private DirectoryInfo GetWorkingFolder()
        {
            return new DirectoryInfo(Environment.CurrentDirectory);            
        }

        /// <summary>
        /// This removes the working indexes for each one specified in the config.
        /// For each one added, we need to manually remove them here to safely delete the indexes before the next tests.
        /// </summary>
        public void RemoveWorkingIndex()
        {           

            var appData = GetWorkingFolder().GetDirectories("App_Data");
            if (appData.Count() > 0)
            {
                var folder = appData.First().GetDirectories("CWSIndexSetTest").First().GetDirectories("Index").First();
                RemoveIndexForSearcher(folder, "CWSSearcher");

                folder = appData.First().GetDirectories("ConvensionNamedTest").First().GetDirectories("Index").First();
                RemoveIndexForSearcher(folder, "ConvensionNamedSearcher");

                folder = appData.First().GetDirectories("FileIndexSet").First().GetDirectories("Index").First();
                RemoveIndexForSearcher(folder, "FileSearcher");
            }
           
        }

        private void RemoveIndex(DirectoryInfo di)
        {
            if (di != null)
            {
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
        /// This craziness will ensure that a folder containing an index will be deleted and remove all existing locks created
        /// by other tests. 
        /// </summary>
        /// <param name="di"></param>
        /// <param name="searcherName"></param>
        private void RemoveIndexForSearcher(DirectoryInfo di, string searcherName)
        {
            if (di != null)
            {
                var searcher = (LuceneExamineSearcher)ExamineManager.Instance.SearchProviderCollection[searcherName];

                try
                {
                    var s = searcher.GetSearcher();
                    var r = s.GetIndexReader();
                    s.Close();
                    r.Close();
                }
                catch (ApplicationException) { }
                catch (NoSuchDirectoryException) { }
                catch (DirectoryNotFoundException) { }

                RemoveIndex(di);
            }         
        }

        /// <summary>
        /// This creates indexes for each index set based on the template
        /// </summary>
        private void CreateIndexesFromTemplate()
        {
            var template = GetTemplateFolder();
            ExamineLuceneIndexes.Instance.Sets.Cast<IndexSet>().ToList()
                .ForEach(x =>
                {                    
                    var di = new DirectoryInfo(x.IndexPath);
                    if (!di.Exists)
                    {
                        di.Create();
                    }

                    var indexDir = di.CreateSubdirectory("Index");
                    template.GetFiles().ToList().ForEach(f => f.CopyTo(Path.Combine(indexDir.FullName, f.Name)));
                });
        }

    }
}
