using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Examine.LuceneEngine;
using Examine.Web.Demo.Models;

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
                ExamineManager.Instance.IndexProviderCollection["Simple2Indexer"].RebuildIndex();
                timer.Stop();

                return View(timer.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError("DataError", ex.Message);
                return View(0);
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
