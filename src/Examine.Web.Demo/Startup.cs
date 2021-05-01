// This Startup file is based on ASP.NET Core new project templates and is included
// as a starting point for DI registration and HTTP request processing pipeline configuration.
// This file will need updated according to the specific scenario of the application being upgraded.
// For more information on ASP.NET Core startup files, see https://docs.microsoft.com/aspnet/core/fundamentals/startup

using System.IO;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Examine.Web.Demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddExamine();

            services.AddControllersWithViews(ConfigureMvcOptions)
                // Newtonsoft.Json is added for compatibility reasons
                // The recommended approach is to use System.Text.Json for serialization
                // Visit the following link for more guidance about moving away from Newtonsoft.Json to System.Text.Json
                // https://docs.microsoft.com/dotnet/standard/serialization/system-text-json-migrate-from-newtonsoft-how-to
                .AddNewtonsoftJson(options =>
                {
                    options.UseMemberCasing();
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            CreateIndexes(app, env);

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void ConfigureMvcOptions(MvcOptions mvcOptions)
        {
        }

        /// <summary>
        /// Creates the application indexes
        /// </summary>
        /// <param name="examineManager"></param>
        private void CreateIndexes(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // TODO: Make this into a builder and must be done in configure services not after!

            var services = app.ApplicationServices;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var examineManager = services.GetRequiredService<IExamineManager>();
            var dirFactory = services.GetRequiredService<FileSystemDirectoryFactory>();
            
            var simple2Indexer = examineManager.AddIndex(
                new LuceneIndex(
                    loggerFactory,
                    "Simple2Indexer",
                    dirFactory.CreateDirectory(
                        new DirectoryInfo(Path.Combine(env.ContentRootPath, "Examine", "Simple2IndexSet")))));

            var secondIndexer = examineManager.AddIndex(
                new LuceneIndex(
                    loggerFactory,
                    "SecondIndexer",
                    dirFactory.CreateDirectory(
                        new DirectoryInfo(Path.Combine(env.ContentRootPath, "Examine", "SecondIndexSet")))));

            var multiSearcher = examineManager.AddSearcher(
                new MultiIndexSearcher(
                    "MultiIndexSearcher",
                    new[] { simple2Indexer, secondIndexer }));
        }
    }
}
