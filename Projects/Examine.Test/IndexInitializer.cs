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
        //static ctor
        static IndexInitializer()
        {
            
        }

        //public static UmbracoExamineSearcher GetUmbracoSearcher(Lucene.Net.Store.Directory luceneDir)
        //{

        //    return new UmbracoExamineSearcher(luceneDir, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
        //}
        
        public static LuceneSearcher GetLuceneSearcher(Lucene.Net.Store.Directory luceneDir)
        {
            return new LuceneSearcher(luceneDir, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
        }

        public static MultiIndexSearcher GetMultiSearcher(Lucene.Net.Store.Directory pdfDir, Lucene.Net.Store.Directory simpleDir, Lucene.Net.Store.Directory conventionDir, Lucene.Net.Store.Directory cwsDir)
        {
            var i = new MultiIndexSearcher(new[] { pdfDir, simpleDir, conventionDir, cwsDir }, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
            return i;
        }


        


    }
}
