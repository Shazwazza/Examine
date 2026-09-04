using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Facet;
using Lucene.Net.Search;
using LuceneFacetResult = Lucene.Net.Facet.FacetResult;

namespace Examine.Lucene.Search
{
    internal sealed class RandomSamplingAmortizedFacets : Facets
    {
        private readonly Facets _innerFacets;
        private readonly RandomSamplingFacetsCollector _samplingCollector;
        private readonly FacetsConfig _facetConfig;
        private readonly IndexSearcher _searcher;

        public RandomSamplingAmortizedFacets(
            Facets innerFacets,
            RandomSamplingFacetsCollector samplingCollector,
            FacetsConfig facetConfig,
            IndexSearcher searcher)
        {
            _innerFacets = innerFacets;
            _samplingCollector = samplingCollector;
            _facetConfig = facetConfig;
            _searcher = searcher;
        }

        public override IList<LuceneFacetResult> GetAllDims(int topN)
        {
            var allDims = _innerFacets.GetAllDims(topN);
            var result = new List<LuceneFacetResult>(allDims.Count);
            for (var i = 0; i < allDims.Count; i++)
            {
                result.Add(Amortize(allDims[i]));
            }

            return result;
        }

        public override float GetSpecificValue(string dim, params string[] path)
        {
            var specificValue = _innerFacets.GetSpecificValue(dim, path);
            if (path.Length == 0)
            {
                return specificValue;
            }

            var parentPath = path.Length > 1
                ? path.Take(path.Length - 1).ToArray()
                : Array.Empty<string>();

            var amortized = Amortize(new LuceneFacetResult(
                dim,
                parentPath,
                specificValue,
                [new LabelAndValue(path[^1], specificValue)],
                1));

            return amortized.LabelValues[0].Value;
        }

        public override LuceneFacetResult GetTopChildren(int topN, string dim, params string[] path)
        {
            var facetResult = _innerFacets.GetTopChildren(topN, dim, path);
#pragma warning disable CS8603
            return facetResult is null ? facetResult : Amortize(facetResult);
#pragma warning restore CS8603
        }

        private LuceneFacetResult Amortize(LuceneFacetResult facetResult)
            => _samplingCollector.AmortizeFacetCounts(facetResult, _facetConfig, _searcher);
    }
}
