using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Examine.Test.DataServices;
using System.Threading;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using Examine.Providers;
using System.Collections.Specialized;
using Examine.LuceneEngine.Config;
using Lucene.Net.Search;

namespace Examine.Test
{
    /// <summary>
    /// Used internally by test classes to initialize a new index from the template
    /// </summary>
    internal static class IndexInitializer
    {

        //private static ConfigIndexCriteria CreateCriteria()
        //{
        //    return new ConfigIndexCriteria(
        //        new[]
        //        {
        //            new ConfigIndexField {Name = "id", EnableSorting = true, Type = "Number"},
        //            new ConfigIndexField {Name = "nodeName", EnableSorting = true},
        //            new ConfigIndexField {Name = "updateDate", EnableSorting = true, Type = "DateTime"},
        //            new ConfigIndexField {Name = "writerName"},
        //            new ConfigIndexField {Name = "path"},
        //            new ConfigIndexField {Name = "nodeTypeAlias"},
        //            new ConfigIndexField {Name = "parentID"},
        //            new ConfigIndexField {Name = "sortOrder", EnableSorting = true, Type = "Number"},
        //        },
        //        new[]
        //        {
        //            new ConfigIndexField {Name = "headerText"},
        //            new ConfigIndexField {Name = "bodyText"},
        //            new ConfigIndexField {Name = "metaDescription"},
        //            new ConfigIndexField {Name = "metaKeywords"},
        //            new ConfigIndexField {Name = "bodyTextColOne"},
        //            new ConfigIndexField {Name = "bodyTextColTwo"},
        //            new ConfigIndexField {Name = "xmlStorageTest"},
        //            new ConfigIndexField {Name = "umbracoNaviHide"}
        //        },
        //        new[]
        //        {
        //            "CWS_Home",
        //            "CWS_Textpage",
        //            "CWS_TextpageTwoCol",
        //            "CWS_NewsEventsList",
        //            "CWS_NewsItem",
        //            "CWS_Gallery",
        //            "CWS_EventItem",
        //            "Image",
        //        },
        //        new string[] { },
        //        -1);
        //}

        //public static UmbracoContentIndexer GetUmbracoIndexer(Lucene.Net.Store.Directory luceneDir)
        //{
        //    var i = new UmbracoContentIndexer(CreateCriteria(),
        //        luceneDir, //custom lucene directory
        //        new TestDataService(),
        //        new CultureInvariantStandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
        //        false);

        //    i.IndexingError += IndexingError;

        //    return i;
        //}

        //public static UmbracoContentIndexer GetUmbracoIndexer(IndexWriter writer)
        //{
        //    var i = new UmbracoContentIndexer(CreateCriteria(),
        //        writer, //global writer
        //        new TestDataService(),
        //        false);

        //    i.IndexingError += IndexingError;

        //    return i;
        //}

        //public static UmbracoExamineSearcher GetUmbracoSearcher(Lucene.Net.Store.Directory luceneDir)
        //{
        //    return new UmbracoExamineSearcher(luceneDir, new CultureInvariantStandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
        //}

        //public static UmbracoExamineSearcher GetUmbracoSearcher(IndexWriter writer)
        //{
        //    return new UmbracoExamineSearcher(writer, new CultureInvariantStandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
        //}

        //public static ValueSetIndexer GetSimpleIndexer(Lucene.Net.Store.Directory luceneDir)
        //{
        //    var i = new ValueSetIndexer(new ConfigIndexCriteria(
        //                                                 new IIndexField[] { },
        //                                                 new[]
        //                                                     {
        //                                                         new TestIndexField { Name = "Author" },
        //                                                         new TestIndexField { Name = "DateCreated", EnableSorting = true, Type = "DateTime"  },
        //                                                         new TestIndexField { Name = "Title" },
        //                                                         new TestIndexField { Name = "Photographer" },
        //                                                         new TestIndexField { Name = "YearCreated", Type = "Date.Year" },
        //                                                         new TestIndexField { Name = "MonthCreated", Type = "Date.Month" },
        //                                                         new TestIndexField { Name = "DayCreated", Type = "Date.Day" },
        //                                                         new TestIndexField { Name = "HourCreated", Type = "Date.Hour" },
        //                                                         new TestIndexField { Name = "MinuteCreated", Type = "Date.Minute" },
        //                                                         new TestIndexField { Name = "SomeNumber", Type = "Number" },
        //                                                         new TestIndexField { Name = "SomeFloat", Type = "Float" },
        //                                                         new TestIndexField { Name = "SomeDouble", Type = "Double" },
        //                                                         new TestIndexField { Name = "SomeLong", Type = "Long" }
        //                                                     },
        //                                                 new string[] { },
        //                                                 new string[] { },
        //                                                 -1),
        //                                                 luceneDir,
        //                                                 new CultureInvariantStandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
        //                                                 new TestValueSetDataProvider(),
        //                                                 new[] { "Documents", "Pictures" },
        //                                                 false);
        //    i.IndexingError += IndexingError;

        //    return i;
        //}

        //public static MultiIndexSearcher GetMultiSearcher(Lucene.Net.Store.Directory pdfDir, Lucene.Net.Store.Directory simpleDir, Lucene.Net.Store.Directory conventionDir, Lucene.Net.Store.Directory cwsDir)
        //{
        //    var i = new MultiIndexSearcher(new[] { pdfDir, simpleDir, conventionDir, cwsDir }, new CultureInvariantStandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
        //    return i;
        //}


        internal static void IndexingError(object sender, IndexingErrorEventArgs e)
        {
            throw new ApplicationException(e.Message, e.InnerException);
        }


    }
}
