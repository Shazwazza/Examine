using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Lucene.Providers;
using Examine.Web.Demo.Models;
using Microsoft.AspNetCore.Mvc;

namespace Examine.Web.Demo.Controllers
{

    public class HomeController : Controller
    {
        private readonly IExamineManager _examineManager;

        public HomeController(IExamineManager examineManager)
        {
            _examineManager = examineManager;
        }

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }

        [HttpGet]
        public ActionResult MultiSearch(string id)
        {
            if (!_examineManager.TryGetSearcher("MultiIndexSearcher", out var multi))
                return NotFound();

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

        [HttpGet]
        public ActionResult Search(string id, string indexName = null)
        {
            if (!_examineManager.TryGetIndex(indexName ?? "MyIndex", out var index))
                return NotFound();

            var searcher = index.Searcher;
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
        public ActionResult RebuildIndex(string indexName = null)
        {
            if (!_examineManager.TryGetIndex(indexName ?? "MyIndex", out var index))
                return NotFound();

            var elapsed = Execute(index, i =>
            {
                try
                {
                    var timer = new Stopwatch();
                    timer.Start();
                    index.CreateIndex();
                    var dataService = new BogusDataService();
                    
                    index.IndexItems(dataService.GetAllData());
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
            if (!_examineManager.TryGetIndex(indexName ?? "MyIndex", out var index))
                return NotFound();

            var items = Execute(index, i =>
            {
                var dataService = new BogusDataService();
                var randomItems = dataService.GetRandomItems(10).ToArray();
                index.IndexItems(randomItems);
                return randomItems.Length;
            });

            return View(items);
        }

        [HttpPost]
        public ActionResult TestIndex(string indexName = null)
        {
            if (!_examineManager.TryGetIndex(indexName ?? "MyIndex", out var index))
                return NotFound();

            if (index is IIndexStats stats)
            {
                var model = new IndexInfo
                {
                    Docs = stats.GetDocumentCount(),
                    Fields = stats.GetFieldNames().Count()
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
            {
                using (luceneIndex.WithThreadingMode(IndexThreadingMode.Synchronous))
                {
                    return action(index);
                }
            }

            return action(index);
        }

    }
}
