using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Examine.AzureDirectory;
using Examine.AzureSearch;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;

namespace Examine.Web.Demo
{
    public class MvcApplication : HttpApplication
    {

        /// <summary>
        /// Creates the application indexes
        /// </summary>
        /// <param name="examineManager"></param>
        public void CreateIndexes(IExamineManager examineManager)
        {
            var adFactory = new AzureDirectoryFactory();

            var simple2Indexer = examineManager.AddIndex(
                new LuceneIndex(
                    "Simple2Indexer",
                    adFactory.CreateDirectory(
                        new DirectoryInfo(Context.Server.MapPath("~/App_Data/Simple2IndexSet")))));

            var secondIndexer = examineManager.AddIndex(
                new LuceneIndex(
                    "SecondIndexer",
                    adFactory.CreateDirectory(
                        new DirectoryInfo(Context.Server.MapPath("~/App_Data/SecondIndexSet")))));

            var azureIndexer = examineManager.AddIndex(
                new AzureSearchIndex("AzureIndex", "examine-test", ConfigurationManager.AppSettings["examine:AzureSearchKey"],

                    //TODO: Azure Search needs a static definition of fields! ack!
                    //However in the Azure Portal it says: Existing fields cannot be changed or deleted. New fields can be added to an existing index at any time.

                    new FieldDefinitionCollection(
                        new FieldDefinition("Column1", FieldDefinitionTypes.FullText),
                        new FieldDefinition("Column2", FieldDefinitionTypes.FullText),
                        new FieldDefinition("Column3", FieldDefinitionTypes.FullText),
                        new FieldDefinition("Column4", FieldDefinitionTypes.FullText),
                        new FieldDefinition("Column5", FieldDefinitionTypes.FullText),
                        new FieldDefinition("Column6", FieldDefinitionTypes.FullText))));

            var multiSearcher = examineManager.AddSearcher(
                new MultiIndexSearcher(
                    "MultiIndexSearcher",
                    new[] {simple2Indexer, secondIndexer}));
        }


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
            Trace.Listeners.Add(new TextWriterTraceListener(
                Context.Server.MapPath("~/App_Data/" + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".log"), "ExamineListener"));

            Trace.WriteLine("App starting");

            CreateIndexes(ExamineManager.Instance);

#if FULLDEBUG
            foreach (var luceneIndexer in ExamineManager.Instance.Indexes.OfType<LuceneIndex>())
            {
                var dir = luceneIndexer.GetLuceneDirectory();
                if (IndexWriter.IsLocked(dir))
                {
                    Trace.WriteLine("Forcing index " + luceneIndexer.Name + " to be unlocked since it was left in a locked state");
                    IndexWriter.Unlock(dir);
                }
            }
#endif

            //take care of unhandled exceptions - there is nothing we can do to
            // prevent the entire w3wp process to go down but at least we can try
            // and log the exception
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var exception = (Exception)args.ExceptionObject;
                var isTerminating = args.IsTerminating; // always true?
                
                Trace.TraceError(exception.ToString());

                Trace.Flush();
            };

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

            // Get the exception object.
            var exc = Server.GetLastError();

            Trace.TraceError(exc.ToString());

            Trace.Flush();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            //Try to log the detailed shutdown message (typical asp.net hack: http://weblogs.asp.net/scottgu/433194)
            try
            {
                var runtime = (HttpRuntime)typeof(HttpRuntime).InvokeMember("_theRuntime",
                            BindingFlags.NonPublic
                            | BindingFlags.Static
                            | BindingFlags.GetField,
                            null,
                            null,
                            null);
                if (runtime == null)
                    return;

                var shutDownMessage = (string)runtime.GetType().InvokeMember("_shutDownMessage",
                    BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.GetField,
                    null,
                    runtime,
                    null);

                var shutDownStack = (string)runtime.GetType().InvokeMember("_shutDownStack",
                    BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.GetField,
                    null,
                    runtime,
                    null);

                var shutdownMsg = $"{HostingEnvironment.ShutdownReason}\r\n\r\n_shutDownMessage={shutDownMessage}\r\n\r\n_shutDownStack={shutDownStack}";

                Trace.TraceInformation("Application shutdown. Details: " + shutdownMsg);
            }
            catch (Exception)
            {
                //if for some reason that fails, then log the normal output
                Trace.TraceInformation("Application shutdown. Reason: " + HostingEnvironment.ShutdownReason);
            }

            Trace.Flush();
        }
    }
}