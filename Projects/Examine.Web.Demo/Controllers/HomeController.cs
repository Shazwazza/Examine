using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Examine.LuceneEngine;
using Examine.LuceneEngine.DataStructures;
using Examine.LuceneEngine.Faceting;
using Examine.Session;
using Examine.Web.Demo.Models;

namespace Examine.Web.Demo.Controllers
{
    public class HomeController : Controller
    {


        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Search(int id)
        {
            var searcher = ExamineManager.Instance.GetSearcher("Simple2Indexer");
            var criteria = searcher.CreateCriteria();
            var result = searcher.Find(criteria.Id(id).Compile());
            
            return View(result);
        }

        [HttpGet]
        public ActionResult SearchLucene(string query)
        {
            var searcher = ExamineManager.Instance.GetSearcher("Simple2Indexer");
            var criteria = searcher.CreateCriteria();
            var result = searcher.Find(criteria.RawQuery(query));

            return View("Search", result);
        }

        [HttpGet]
        public ActionResult SearchCustom(string indexName, string q = null, int count = 10, bool all = false)
        {
            var searcher = ExamineManager.Instance.GetSearcher(indexName);
            
            ILuceneSearchResults result;
            if (all)
            {
                result = searcher.Find(searcher.CreateCriteria().All().Compile());
            }
            else
            {
                result = searcher.Find(q, false);
            }

            return View("Search", result);
        }

        [HttpGet]
        public ActionResult SearchFacets(
            string q,
            string field = "Manufacturer,Category,Model,Description",
            int count = 10, 
            bool countFacets = true, 
            bool facetFilter = false,             
            bool all = false 
            /*TODO: Not used right now
            double likeWeight = 0*/)
        {
            if (string.IsNullOrWhiteSpace(q) && !all) throw new ArgumentException("The search text is not specified");

            var model = new FacetSearchModel();

            var sw = new Stopwatch();
            sw.Start();
            var searcher = ExamineManager.Instance.GetSearcher("Simple2Indexer");
            
            
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
                    //filter based on facets

                    var searchTxts = q.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    var fields = field.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    if (searchTxts.Length != fields.Length) throw new InvalidOperationException("Search texts and field counts must match");

                    var facetKeys = fields.Select((x, i) => new FacetKey(x, searchTxts[i])).ToArray();

                    criteria
                        .Facets(facetKeys)
                        .Compile();
                        //TODO: I think we need to score like this for facet filter/search    
                        //Here, zero means that we don't care about Lucene's score. We only want to know how well the results compare to the facets
                        //.WrapRelevanceScore(0, new FacetKeyLevel("Column4", "Root/Tax1/Tax2", 1));

                        //TODO: Determine if this is working as it should, I think Niels K said it might not be, can't remember
                        ////Score by the like count we have in the external in-memory data.
                        ////The value is normalized. Here we know that we can't have more than 1000 likes. 
                        ////Generally be careful about the scale of the scores you combine. 
                        ////If you compare large numbers to small numbers use a logarithmic transform on the large one (e.g. comparing likes to number of comments)
                        //.WrapExternalDataScore<TestExternalData>(new ScoreAdder(1 - likeWeight), d => d.Likes / 1000f); 
                }
                else
                {
                    //Add column1 filter as normal field query
                    //criteria.Field("Column1", q);
                    criteria.ManagedQuery(q, fields: field.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries))
                        .Compile();
                        //TODO: Determine if this is working as it should, I think Niels K said it might not be, can't remember
                        //.WrapExternalDataScore<TestExternalData>(1 - likeWeight, d => d.Likes / 1000f); 
                }
            }
            
            //Get search results
            var searchResults = searcher.Find(criteria);

            model.SearchResult = searchResults;
            model.CountFacets = countFacets;
            model.Watch = sw;
            model.FacetMap = searchResults.CriteriaContext.FacetMap;
            
            return View(model);

        }

        [HttpPost]
        public ActionResult RebuildIndex()
        {
            var timer = new Stopwatch();            
            using (BogusIndexDataService.PrefetchData(BogusIndexDataService.IndexCategories))
            {
                timer.Start();
                var index = ExamineManager.Instance.IndexProviders["Simple2Indexer"];
                index.RebuildIndex();
                timer.Stop();
            }

            ExamineSession.WaitForChanges();

            var searcher = ExamineManager.Instance.GetSearcher("Simple2Indexer");
            var result = searcher.Find(searcher.CreateCriteria().All().Compile());

            return View(new RebuildModel
            {
                TotalIndexed = result.TotalItemCount,
                TotalSeconds = timer.Elapsed.TotalSeconds
            });
        }

        [HttpPost]
        public ActionResult ReIndexEachItemIndividually()
        {
            try
            {
                var timer = new Stopwatch();
                timer.Start();
                var ds = new BogusIndexDataService();
                foreach (var i in ds.GetAllData("TestType"))
                {
                    ExamineManager.Instance.IndexProviders["Simple2Indexer"].IndexItem(i);                        
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
