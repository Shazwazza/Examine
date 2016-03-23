using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Providers;
using Examine.Session;
using Examine.Web.Demo.Models;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Serilog;
using Version = Lucene.Net.Util.Version;

namespace Examine.Web.Demo
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {            
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }
        
        protected void Application_Start()
        {
            var log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ColoredConsole()
                .WriteTo.RollingFile(Context.Server.MapPath("~/App_Data/Logs/") + "Log-{Date}.txt")
                .CreateLogger();

            var traceListener = new SerilogTraceListener.SerilogTraceListener(log);
            Trace.Listeners.Add(traceListener);

            log.Debug("Application Starting (serilog)");
            Trace.WriteLine("Application Starting");

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //Database.SetInitializer<MyDbContext>(null);

            ConfigureExamine();
        }

        protected void ConfigureExamine()
        {

            var customIndexer = new ValueSetIndexer(
                new[] {new FieldDefinition("Email", FieldDefinitionTypes.Raw)},
                new TestDataService(), 
                new[] {"Type1", "Type2"}, 
                new DirectoryInfo(Context.Server.MapPath("~/App_Data/{machinename}/RuntimeIndex1")),
                new StandardAnalyzer(Version.LUCENE_30));
            
            ExamineManager.Instance.AddIndexProvider("RuntimeIndexer1", customIndexer);

            var sqlIndexer = new ValueSetIndexer(
                new[]
                {
                    new FieldDefinition("Column1", FieldDefinitionTypes.Raw),
                    new FieldDefinition("Column1", "Column1_Facet", FieldDefinitionTypes.Facet),
                    new FieldDefinition("Column2", FieldDefinitionTypes.Facet),
                    new FieldDefinition("Column3", FieldDefinitionTypes.Raw),
                    new FieldDefinition("Column4", FieldDefinitionTypes.FacetPath),
                    new FieldDefinition("Column5", FieldDefinitionTypes.FullText),
                    new FieldDefinition("Column6", FieldDefinitionTypes.FullText)
                },
                new TableDirectReaderDataService(), 
                new[] { "TestType" },
                new DirectoryInfo(Context.Server.MapPath("~/App_Data/{machinename}/SimpleIndexSet2")),
                new StandardAnalyzer(Version.LUCENE_30));

            ExamineManager.Instance.AddIndexProvider("Simple2Indexer", sqlIndexer);

            //This is how to create a config from code if you had indexes declared in with configuration.
            //This allows your own termfacetextractors to be used.
            //Note that this must be added to index sets BEFORE ExamineManager.Instance is accessed.

            //var indexSet = IndexSets.Instance.Sets["Simple2IndexSet"];
            ////Here a facet extractor is configured from code
            //var config = indexSet.FacetConfiguration = indexSet.FacetConfiguration ?? new FacetConfiguration();
            //config.FacetExtractors.Add(new TermFacetExtractor("CustomDocField"));
            //config.FacetExtractors.Add(new TermFacetExtractor("RefFacet", valuesAreReferences: true));

            ////Attach in-memory objects to lucene documents for scoring on rapidly changing data.
            //config.ExternalDataProvider = new TestExternalDataProvider();
            
            //And we're ready.

            var indexer = ExamineManager.Instance.IndexProviders["Simple2Indexer"] as LuceneIndexer;
            
            var r = new Random();
            //Here custom fields are written directly to the document regardsless of Examine's config
            indexer.DocumentWriting += (sender, args) =>
            {
                string v;
                if (args.Fields.TryGetValue("Column1", out v))
                {
                    //Here the umbraco value of a tag picker could be split into individual tags  
                    
                    //This is how to add a facet with level (i.e. size/importance)
                    //Remember to use a float value. Not int.

                    foreach (var f in Enumerable.Range(0, r.Next(1, 5)).Select(i => r.Next(1, 27000)).Distinct())
                    {
                        args.Document.Add(new Field("RefFacet", new ReferenceFacetValue(f).TokenStream));
                    }

                    args.Document.Add(new Field("CustomDocField", TokenStreamHelper.Create(v + "_WithLevel", .25f)));

                    //Here we add a normal field
                    args.Document.Add(new Field("CustomDocField", v + "Test2", Field.Store.NO, Field.Index.NOT_ANALYZED));
                }
            };

            //searcher.FacetConfiguration = new FacetConfiguration
            ////    { 
            ////        FacetExtractors = new List<IFacetExtractor> {new TermFacetExtractor("Column1")}
            ////    };
        }


        protected void Application_End()
        {
            Trace.WriteLine("Application Ended");
        }
    }
}