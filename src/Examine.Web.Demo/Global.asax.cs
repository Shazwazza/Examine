using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;

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
            Trace.Listeners.Add(new TextWriterTraceListener(
                Context.Server.MapPath("~/App_Data/" + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".log"), "ExamineListener"));

            Trace.WriteLine("App starting");

#if FULLDEBUG
            foreach (var luceneIndexer in ExamineManager.Instance.IndexProviders.OfType<LuceneIndexer>())
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

                var shutdownMsg = string.Format("{0}\r\n\r\n_shutDownMessage={1}\r\n\r\n_shutDownStack={2}",
                    HostingEnvironment.ShutdownReason,
                    shutDownMessage,
                    shutDownStack);

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