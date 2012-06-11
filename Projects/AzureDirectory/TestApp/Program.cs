using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Lucene.Net;
using Lucene.Net.Store;
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Util;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using System.Diagnostics;
using System.ComponentModel;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace TestApp
{
    class Program
    {
        public static bool fExit = false;

        static void Main(string[] args)
        {
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

            
            // default AzureDirectory stores cache in local temp folder
            AzureDirectory azureDirectory = new AzureDirectory(CloudStorageAccount.FromConfigurationSetting("blobStorage"), "TestCatalog6");
            bool findexExists = IndexReader.IndexExists(azureDirectory);

            IndexWriter indexWriter = null;
            while (indexWriter == null)
            {
                try
                {
                    indexWriter = new IndexWriter(azureDirectory, new StandardAnalyzer(), !IndexReader.IndexExists(azureDirectory));
                }
                catch (LockObtainFailedException)
                {
                    Console.WriteLine("Lock is taken, Hit 'Y' to clear the lock, or anything else to try again");
                    if (Console.ReadLine().ToLower().Trim() == "y" )
                        azureDirectory.ClearLock("write.lock");
                }
            };
            Console.WriteLine("IndexWriter lock obtained, this process has exclusive write access to index");
            indexWriter.SetRAMBufferSizeMB(10.0);
            indexWriter.SetUseCompoundFile(false);
            indexWriter.SetMaxMergeDocs(10000);
            indexWriter.SetMergeFactor(100);
            
            for (int iDoc = 0; iDoc < 10000; iDoc++)
            {
                if (iDoc % 10 == 0)
                    Console.WriteLine(iDoc);
                Document doc = new Document();
                doc.Add(new Field("id", DateTime.Now.ToFileTimeUtc().ToString(), Field.Store.YES, Field.Index.TOKENIZED, Field.TermVector.NO));
                doc.Add(new Field("Title", GeneratePhrase(10), Field.Store.YES, Field.Index.TOKENIZED, Field.TermVector.NO));
                doc.Add(new Field("Body", GeneratePhrase(40), Field.Store.YES, Field.Index.TOKENIZED, Field.TermVector.NO));
                indexWriter.AddDocument(doc);
            }
            Console.WriteLine("Total docs is {0}", indexWriter.DocCount());
            indexWriter.Close();

            IndexSearcher searcher;
            using (new AutoStopWatch("Creating searcher"))
            {
                searcher = new IndexSearcher(azureDirectory); 
            }
            SearchForPhrase(searcher, "dog");
            SearchForPhrase(searcher, _random.Next(32768).ToString());
            SearchForPhrase(searcher, _random.Next(32768).ToString());
        }


        static void SearchForPhrase(IndexSearcher searcher, string phrase)
        {
            using (new AutoStopWatch(string.Format("Search for {0}", phrase)))
            {
                Lucene.Net.QueryParsers.QueryParser parser = new Lucene.Net.QueryParsers.QueryParser("Body", new StandardAnalyzer());
                Lucene.Net.Search.Query query = parser.Parse(phrase);

                Hits hits = searcher.Search(query);
                Console.WriteLine("Found {0} results for {1}", hits.Length(), phrase);
                int max = hits.Length();
                if (max > 100) 
                    max = 100;
                for (int i = 0; i < max; i++)
                {
                    Console.WriteLine(hits.Doc(i).GetField("Title").StringValue());
                }
            }
        }

        static Random _random = new Random((int)DateTime.Now.Ticks);
        static string[] sampleTerms =
            { 
                "dog","cat","car","horse","door","tree","chair","microsoft","apple","adobe","google","golf","linux","windows","firefox","mouse","hornet","monkey","giraffe","computer","monitor",
                "steve","fred","lili","albert","tom","shane","gerald","chris",
                "love","hate","scared","fast","slow","new","old"
            };

        private static string GeneratePhrase(int MaxTerms)
        {
            StringBuilder phrase = new StringBuilder();
            int nWords = 2 + _random.Next(MaxTerms);
            for (int i = 0; i < nWords; i++)
            {
                phrase.AppendFormat(" {0} {1}", sampleTerms[_random.Next(sampleTerms.Length)], _random.Next(32768).ToString() );
            }
            return phrase.ToString();
        }

    }
    public class AutoStopWatch : IDisposable
    {
        private Stopwatch _stopwatch;
        private string _message;
        public AutoStopWatch(string message)
        {
            _message = message;
            Debug.WriteLine(String.Format("{0} starting ", message));
            _stopwatch = Stopwatch.StartNew();
        }


        #region IDisposable Members
        public void Dispose()
        {

            _stopwatch.Stop();
            long ms = _stopwatch.ElapsedMilliseconds;

            Debug.WriteLine(String.Format("{0} Finished {1} ms", _message, ms));
        }
        #endregion
    }


}
