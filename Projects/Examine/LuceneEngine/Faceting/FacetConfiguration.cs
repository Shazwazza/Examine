using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// Facet configuration
    /// </summary>
    public class FacetConfiguration
    {
        private readonly HashSet<IFacetExtractor> _extractors = new HashSet<IFacetExtractor>(); 

        /// <summary>
        /// Constructor
        /// </summary>
        public FacetConfiguration()
        {
            FacetMap = new FacetMap();
            FacetCombs = new List<IFacetComb>() { new MaxLevelFacetComb() };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="extractors"></param>
        public FacetConfiguration(IEnumerable<IFacetExtractor> extractors)
        {
            _extractors = new HashSet<IFacetExtractor>(extractors);
            FacetMap = new FacetMap();
            FacetCombs = new List<IFacetComb>() { new MaxLevelFacetComb() };
        }

        /// <summary>
        /// Constructor - we won't expose this yet until we enable IExternalDataProvider
        /// </summary>
        /// <param name="facetMap"></param>
        /// <param name="externalDataProvider"></param>
        /// <param name="cacheAllQueryFilters"></param>
        internal FacetConfiguration(FacetMap facetMap, IExternalDataProvider externalDataProvider, bool cacheAllQueryFilters)
        {
            FacetMap = facetMap;
            ExternalDataProvider = externalDataProvider;
            CacheAllQueryFilters = cacheAllQueryFilters;
        }

        /// <summary>
        /// Returns true if there are no defined facet extractors
        /// </summary>
        public bool IsEmpty => !FacetExtractors.Any();

        /// <summary>
        /// Gets the list of facet extractors
        /// </summary>
        public IEnumerable<IFacetExtractor> FacetExtractors => _extractors;

        internal IEnumerable<IFacetComb> FacetCombs { get; private set; }

        /// <summary>
        /// Used internally to add extractors to an existing instance - during startup
        /// </summary>
        /// <param name="extractor"></param>
        internal void AddExtractor(IFacetExtractor extractor)
        {
            _extractors.Add(extractor);
        }

        /// <summary>
        /// Returns the FacetMap associated with this configuration
        /// </summary>
        public FacetMap FacetMap { get; private set; }

        /// <summary>
        /// Returns the currently associated IExternalDataProvider if one has been assigned
        /// </summary>
        //TODO: We need to investigate this
        internal IExternalDataProvider ExternalDataProvider { get; private set; }

        //TODO: This is used in ReaderData.ReadFacets, but it's never set to true... when do we want this?
        public bool CacheAllQueryFilters { get; private set; }

        
    }
}
