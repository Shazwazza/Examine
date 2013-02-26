using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Providers;
using Examine.Web.Demo.Models;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Index;
using Lucene.Net.Search;

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

                                var r = new Random(1337);

                                for (var i = 0; i < 27000; i++)
                                {
                                    rec.SetString(1, "a" + r.Next(0, 10));
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

        public ActionResult Search(string q, int count = 10, bool countFacets = true, bool facetFilter = true, bool all = false)
        {
            var sw = new Stopwatch();
            sw.Start();
            var searcher = ExamineManager.Instance.SearchProviderCollection["Simple2Searcher"];

            //This is for text/plain output
            var sb = new StringBuilder();

            //Create a basic criteria with the options from the query string
            var criteria = searcher.CreateSearchCriteria().MaxCount(count).CountFacets(countFacets);

            if (all)
            {
                criteria.All();
            }
            else
            {
                if (facetFilter)
                {
                    //Add column1 filter as facet filter
                    criteria.Facet(new FacetKey("Column1", q));
                }
                else
                {             
                    //Add column1 filter as normal field query
                    criteria.Field("Column1", q);
                }
            }

            //Get search results
            var searchResults = criteria.Execute();


            sb.Append("Total hits: " + searchResults.TotalItemCount + "\r\n");

            //Show the results (limited by criteria.MaxCount(...) or SearchOptions.Default.MaxCount)
            foreach (var res in searchResults)
            {
                sb.Append(res.Id + "\r\n");
            }

            if (countFacets) //If false FacetCounts is null
            {
                //Iterate all facets and show their key and count.
                foreach (var res in searchResults.FacetCounts)
                {
                    sb.Append(res.Key + ": " + res.Value + "\r\n");
                }
            }


            //var ls = (LuceneSearcher) searcher;
            //var ctx = ls.GetSearcherContext();
            //var map = ctx.Searcher.FacetConfiguration.FacetMap;
            //foreach( var f in ctx.Searcher.FacetConfiguration.FacetMap.Keys)
            //{
            //    sb.Append(f.ToString() + ": " + map.GetIndex(f) + "\r\n");
            //}

            //foreach( var d in ctx.ReaderData)
            //{
            //    sb.Append(d.Value.FacetLevels.Length + "\r\n");
            //}


            sw.Stop();

            sb.AppendFormat("Elapsed {0:N2} ms.", sw.Elapsed.TotalMilliseconds);

            return Content(sb.ToString(), "text/plain");
        }

        [HttpPost]
        public ActionResult RebuildIndex()
        {
            try
            {
                var timer = new Stopwatch();
                timer.Start();
                ExamineManager.Instance.IndexProviderCollection["Simple2Indexer"].RebuildIndex();
                timer.Stop();

                return View(timer.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError("DataError", ex.Message + " - " + ex.InnerException.ToString());
                return View(0.0);
            }
        }

        [HttpPost]
        public ActionResult ReIndexEachItemIndividually()
        {
            try
            {
                var timer = new Stopwatch();
                timer.Start();
                var ds = new TableDirectReaderDataService();
                foreach (var i in ds.GetAllData("TestType"))
                {
                    ExamineManager.Instance.IndexProviderCollection["Simple2Indexer"]
                        .ReIndexNode(i.RowData.ToExamineXml(i.NodeDefinition.NodeId, i.NodeDefinition.Type), "TestType");
                }
                timer.Stop();

                return View(timer.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError("DataError", ex.Message);
                return View(0);
            }
        }


    }
}
