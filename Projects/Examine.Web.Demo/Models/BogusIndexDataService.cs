using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Indexing;

namespace Examine.Web.Demo.Models
{
    
    public class BogusIndexDataService : IValueSetService
    {
        public static string[] IndexCategories = new[] { "SLR", "DSLR", "Compact" };
     
        public static IDisposable PrefetchData(string[] categories)
        {
            Prefetch.Build(categories);
            return Prefetch;
        }

        private static readonly DataPrefetch Prefetch = new DataPrefetch();

        private class DataPrefetch : IDisposable
        {
            public void Build(string[] categories)
            {   
                var dataService = new BogusIndexDataService();
                DataSet = categories
                    .Select((x, i) => new KeyValuePair<string, ValueSet[]>(x, dataService.CreateData(x, i).ToArray()))
                    .ToDictionary(x => x.Key, x => x.Value);     
            }

            public Dictionary<string, ValueSet[]> DataSet { get; private set; }
            public bool HasData => DataSet != null;

            public void Dispose()
            {
                DataSet.Clear();
                DataSet = null;
            }
        }

        public IEnumerable<ValueSet> GetAllData(string indexCategory)
        {
            if (!Prefetch.HasData) throw new InvalidOperationException("The data service has not prefetched it's data");
            return Prefetch.DataSet[indexCategory];
        }

        private IEnumerable<ValueSet> CreateData(string indexCategory, int categoryIndex)
        {
            var ids = 9000 * categoryIndex;
            var manufacturer = new[] { "Canon", "Sony", "Nikon", "Pentax", "Fuji", "Kodak", "Olympus", "Casio" };
            var color = new[] {"black", "white", "blue", "green", "yellow", "orange", "green", "purple", "grey", "pink", "magenta", "teal", "taupe", "beige", "lavendar"};

            var values = new Faker<TestModel>()                
                .StrictMode(true)
                .RuleFor(o => o.Id, f => ids++)
                .RuleFor(o => o.Category, f => indexCategory)
                .RuleFor(o => o.Manufacturer, f => f.PickRandom(manufacturer))
                .RuleFor(o => o.MegaPixels, f => f.Random.Number(1, 25))
                .RuleFor(o => o.Model, f => f.Lorem.Word())
                .RuleFor(o => o.Description, f => f.Lorem.Sentence())
                .RuleFor(o => o.Price, f => f.Finance.Amount(50, decimals:0))
                .RuleFor(o => o.ReleaseDate, f => f.Date.Between(DateTime.Now - TimeSpan.FromDays(100), DateTime.Now + TimeSpan.FromDays(100)).Date)
                .RuleFor(o => o.Color, f => f.PickRandom(color))
                .RuleFor(o => o.InStock, f => f.Random.Number(1, 10));

            return values
                //9000 per category
                .Generate(9000)
                .Select(x => new ValueSet(Convert.ToInt64(x.Id), x.Category,
                    ObjectExtensions.ConvertObjectToDictionary(x,
                        //Do not include these properties in the dictionary    
                        new[] {"Id"})));
        }
    }
}