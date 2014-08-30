using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Faceting;
using Examine.Session;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.Scoring;
using Examine.Web.Demo.Models;
using Examine.LuceneEngine.SearchCriteria;
using Lucene.Net.Index;
using Lucene.Net.Search;
using NLipsum.Core;

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
                        var r = new Random();
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
                                    var path = new List<string> { "Root" };
                                    for (int j = 0, n = r.Next(1, 3); j < n; j++)
                                    {
                                        path.Add("Tax" + r.Next(0, 5));
                                    }

                                    rec.SetString(1, "a" + r.Next(0, 10));
                                    rec.SetString(2, "b" + r.Next(0, 100));
                                    rec.SetString(3, "c" + i);
                                    rec.SetString(4, string.Join("/", path));
                                    rec.SetString(5, LipsumGenerator.GenerateHtml(r.Next(1, 5)));
                                    rec.SetString(6, "This is a nice little test. Made by Kühnel");
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

        public ActionResult Search(string q = null, int count = 10, bool countFacets = true, bool facetFilter = true, bool all = false, double likeWeight = 0)
        {
            var sw = new Stopwatch();
            sw.Start();
            var searcher = ExamineManager.Instance.GetLuceneSearcher("Simple2Searcher");

            //This is for text/plain output
            var sb = new StringBuilder();
            
            //Create a basic criteria with the options from the query string
            var criteria = searcher.CreateCriteria()
                                   .MaxCount(count)
                                   .CountFacets(countFacets)
                                   .CountFacetReferences(true);

            if (all || string.IsNullOrEmpty(q))
            {
                criteria.All();
            }
            else
            {
                if (facetFilter)
                {
                    //Add column1 filter as facet filter
                    criteria.Facets(new FacetKey("Column1_Facet", q))
                        .Compile()
                        //Here, zero means that we don't case about Lucene's score. We only want to know how well the results compare to the facets
                        .WrapRelevanceScore(0, new FacetKeyLevel("Column4", "Root/Tax1/Tax2", 1))

                        //Score by the like count we have in the external in-memory data.
                        //The value is normalized. Here we know that we can't have more than 1000 likes. 
                        //Generally be careful about the scale of the scores you combine. 
                        //If you compare large numbers to small numbers use a logarithmic transform on the large one (e.g. comparing likes to number of comments)
                        .WrapExternalDataScore<TestExternalData>(new ScoreAdder(1 - likeWeight), d => d.Likes / 1000f); 
                }
                else
                {
                    //Add column1 filter as normal field query
                    //criteria.Field("Column1", q);
                    criteria.ManagedQuery(q, fields: new[]{"Column5", "Column6"})
                        //.Or().Field("Column6", "test")
                        .Compile()
                        .WrapExternalDataScore<TestExternalData>(1 - likeWeight, d => d.Likes / 1000f); 
                }
            }
            
            //Get search results
            var searchResults = searcher.Find(criteria);
            
            sb.Append("Total hits: " + searchResults.TotalItemCount + "\r\n");


            //Show the results (limited by criteria.MaxCount(...) or SearchOptions.Default.MaxCount)
            foreach (var res in searchResults)
            {
                sb.AppendLine();
                sb.AppendLine("ID: " + res.LongId);
                sb.AppendLine(res.GetHighlight("Column5"));
                sb.AppendLine(res.GetHighlight("Column6"));
                sb.Append("   Facets: ");
                sb.AppendLine(string.Join(", ", res.Facets.Select(l => l.FacetId + ":" + l.Level.ToString("N2"))));
                sb.AppendLine("   Likes: " + ((TestExternalData) TestExternalDataProvider.Instance.GetData(res.LongId)).Likes);

                if (res.FacetCounts != null)
                {
                    foreach (var fc in res.FacetCounts)
                    {
                        sb.AppendLine(fc.FieldName + ": " + fc.Count);
                    }
                }
            }

            sw.Stop();

            var map = searchResults.CriteriaContext.FacetMap;

            if (countFacets) //If false FacetCounts is null
            {
                TextWriter output;
                //Iterate all facets and show their key and count.
                foreach (var res in searchResults.FacetCounts.GetTopFacets(10))
                {
                    sb.Append(res.Key.FieldName + ":" + res.Key.Value + ", count = " + res.Count + ", ");                    
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


            

            sb.AppendFormat("Elapsed {0:N2} ms.", sw.Elapsed.TotalMilliseconds);


            if (map != null)
            {
                sb.Append("\r\n\r\nField names:\r\n");
                foreach (var f in map.FieldNames)
                {
                    sb.Append(f);

                    foreach (var val in map.GetByFieldNames(f))
                    {
                        sb.Append(val.Value + ",  ");
                    }

                    sb.Append("\r\n\r\n");
                }
            }
            return Content(sb.ToString(), "text/plain");
        }

        [HttpPost]
        public ActionResult RebuildIndex()
        {
            //try
            //{
            var timer = new Stopwatch();
            timer.Start();
            ExamineManager.Instance.IndexProviderCollection["Simple2Indexer"].RebuildIndex();
            timer.Stop();

            return View(timer.Elapsed.TotalSeconds);
            //}
            //catch (Exception ex)
            //{
            //    this.ModelState.AddModelError("DataError", ex.Message + (ex.InnerException != null ? " - " + ex.InnerException : ""));
            //    return View(0.0);
            //}
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
                
                ExamineSession.WaitForChanges();

                return View(timer.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError("DataError", ex.Message);
                return View(0d);
            }
        }


    }
}
