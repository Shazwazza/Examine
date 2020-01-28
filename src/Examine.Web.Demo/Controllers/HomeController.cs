using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Web.Mvc;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Examine.Web.Demo.Models;
using Lucene.Net.Index;

namespace Examine.Web.Demo.Controllers
{
    public class HomeController : Controller
    {


        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        [ValidateInput(false)]
        [HttpGet]
        public ActionResult MultiSearch(string id)
        {
            if (!AspExamineManager.Instance.TryGetSearcher("MultiIndexSearcher", out var multi))
                return HttpNotFound();

            var criteria = multi.CreateQuery();
            var result = criteria.NativeQuery(id).Execute();

            var sb = new StringBuilder();
            sb.AppendLine($"Results :{result.TotalItemCount}");
            foreach (var searchResult in result)
            {
                sb.AppendLine($"Id:{searchResult.Id}, Score:{searchResult.Score}, Vals: {string.Join(", ", searchResult.Values.Select(x => x.Value))}");
            }
            return Content(sb.ToString());
        }

        [ValidateInput(false)]
        [HttpGet]
        public ActionResult Search(string id, string indexName = null)
        {
            if (!AspExamineManager.Instance.TryGetIndex(indexName ?? "Simple2Indexer", out var index))
                return HttpNotFound();

            var searcher = index.GetSearcher();
            var criteria = searcher.CreateQuery();
            var result = criteria.NativeQuery(id).Execute();
            var sb = new StringBuilder();
            sb.AppendLine($"Results :{result.TotalItemCount}");
            foreach (var searchResult in result)
            {
                sb.AppendLine($"Id:{searchResult.Id}, Score:{searchResult.Score}, Vals: {string.Join(", ", searchResult.Values.Select(x => x.Value))}");
            }
            return Content(sb.ToString());
        }

        [HttpPost]
        public ActionResult Populate()
        {
            try
            {
                using (var db = new MyDbContext())
                {
                    //check if we have data
                    if (!db.TestModels.Any())
                    {
                        //using TableDirect is BY FAR one of the fastest ways to bulk insert data in SqlCe... 
                        using (db.Database.Connection)
                        {
                            db.Database.Connection.Open();
                            using (var cmd = (SqlCeCommand)db.Database.Connection.CreateCommand())
                            {
                                cmd.CommandText = "TestModels";
                                cmd.CommandType = CommandType.TableDirect;

                                var rs = cmd.ExecuteResultSet(ResultSetOptions.Updatable);
                                var rec = rs.CreateRecord();

                                for (var i = 0; i < 27000; i++)
                                {
                                    rec.SetString(1, "a" + i);
                                    rec.SetString(2, "b" + i);
                                    rec.SetString(3, "c" + i);
                                    rec.SetString(4, "d" + i);
                                    rec.SetString(5, "e" + i);
                                    rec.SetString(6, "f" + i);
                                    rs.Insert(rec);
                                }
                            }
                        }
                        return View(true);
                    }
                    else
                    {
                        this.ModelState.AddModelError("DataError", "The database has already been populated with data");
                        return View(false);
                    }
                }
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError("DataError", ex.Message);
                return View(false);
            }
            
        }

        [HttpPost]
        public ActionResult RebuildIndex(string indexName = null)
        {
            if (!AspExamineManager.Instance.TryGetIndex(indexName ?? "Simple2Indexer", out var index))
                return HttpNotFound();

            var elapsed = Execute(index, i =>
            {
                try
                {
                    var timer = new Stopwatch();
                    timer.Start();
                    index.CreateIndex();
                    var dataService = new TableDirectReaderDataService();
                    //NOTE: Max 10k for now since that is the azure search limit for testing.
                    index.IndexItems(dataService.GetAllData().Take(10000));
                    timer.Stop();

                    return timer.Elapsed.TotalSeconds;
                }
                catch (Exception ex)
                {
                    throw;
                }
            });

            return View(elapsed);
        }


        [HttpPost]
        public ActionResult ReIndexItems(string indexName = null)
        {
            if (!AspExamineManager.Instance.TryGetIndex(indexName ?? "Simple2Indexer", out var index))
                return HttpNotFound();

            var items = Execute(index, i =>
            {
                var dataService = new TableDirectReaderDataService();
                var randomItems = dataService.GetRandomItems(10).ToArray();
                index.IndexItems(randomItems);
                return randomItems.Length;
            });

            return View(items);
        }

        [HttpPost]
        public async Task<ActionResult> TestIndex(string indexName = null)
        {
            if (!AspExamineManager.Instance.TryGetIndex(indexName ?? "Simple2Indexer", out var index))
                return HttpNotFound();

            if (index is IIndexStats stats)
            {
                var model = new IndexInfo
                {
                    Docs = await stats.GetDocumentCountAsync(),
                    Fields = (await stats.GetFieldNamesAsync()).Count()
                };
                return View(model);
            }

            throw new InvalidCastException("The index was not IIndexStats");
        }


        /// <summary>
        /// Just checks if it's a lucene index and if so processes non async (for testing purposes)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="action"></param>
        private T Execute<T>(IIndex index, Func<IIndex, T> action)
        {
            if (index is LuceneIndex luceneIndex)
                using (luceneIndex.ProcessNonAsync())
                    return action(index);

            return action(index);
        }

    }
}
