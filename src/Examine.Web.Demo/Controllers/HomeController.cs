using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        [HttpGet]
        public ActionResult MultiSearch(string query)
        {
            var multi = ExamineManager.Instance.GetRegisteredSearcher("MultiIndexSearcher");
            var criteria = multi.CreateCriteria();
            var result = multi.Search(criteria.RawQuery(query));

            var sb = new StringBuilder();
            sb.AppendLine($"Results :{result.TotalItemCount}");
            foreach (var searchResult in result)
            {
                sb.AppendLine($"Id:{searchResult.Id}, Score:{searchResult.Score}, Vals: {string.Join(", ", searchResult.Fields.Select(x => x.Value))}");
            }
            return Content(sb.ToString());
        }

        [HttpGet]
        public ActionResult Search(string id)
        {
            var searcher = ExamineManager.Instance.GetSearcher("Simple2Indexer");
            var criteria = searcher.CreateCriteria();
            var result = searcher.Search(criteria.RawQuery(id));
            var sb = new StringBuilder();
            sb.AppendLine($"Results :{result.TotalItemCount}");
            foreach (var searchResult in result)
            {
                sb.AppendLine($"Id:{searchResult.Id}, Score:{searchResult.Score}, Vals: {string.Join(", ", searchResult.Fields.Select(x => x.Value))}");
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
        public ActionResult RebuildIndex()
        {
            try
            {
                var timer = new Stopwatch();
                timer.Start();
                ExamineManager.Instance.IndexProviders["Simple2Indexer"].RebuildIndex();
                timer.Stop();

                return View(timer.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError("DataError", ex.Message);
                return View(0.0);
            }
        }

        [HttpPost]
        public ActionResult ReIndexItems()
        {
            var dataService = new TableDirectReaderDataService();
            var randomItems = dataService.GetRandomItems().ToArray();
            ExamineManager.Instance.IndexProviders["Simple2Indexer"].IndexItems(randomItems);
            return View(randomItems.Length);
        }

        [HttpPost]
        public ActionResult TestIndex()
        {

            var indexer = (LuceneIndexer)ExamineManager.Instance.IndexProviders["Simple2Indexer"];
            var writer = indexer.GetIndexWriter();

            var model = new IndexInfo
            {
                Docs = writer.NumDocs(),
                Fields = writer.GetReader().GetFieldNames(IndexReader.FieldOption.ALL).Count
            };

            return View(model);
            
        }


    }
}
