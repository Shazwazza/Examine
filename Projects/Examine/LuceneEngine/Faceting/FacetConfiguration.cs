using System.Collections.Generic;
using System.Security;

namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// Facet configuration
    /// </summary>
    public class FacetConfiguration
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FacetConfiguration()
        {
            FacetMap = new FacetMap();
            FacetExtractors = new List<IFacetExtractor>();
            FacetCombs = new List<IFacetComb>() { new MaxLevelFacetComb() };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="facetMap"></param>
        /// <param name="externalDataProvider"></param>
        /// <param name="cacheAllQueryFilters"></param>
        public FacetConfiguration(FacetMap facetMap, IExternalDataProvider externalDataProvider, bool cacheAllQueryFilters)
        {
            FacetMap = facetMap;
            ExternalDataProvider = externalDataProvider;
            CacheAllQueryFilters = cacheAllQueryFilters;
        }

        /// <summary>
        /// Returns true if there are no defined facet extractors
        /// </summary>
        public bool IsEmpty => FacetExtractors.Count == 0;

        //TODO: Make this immutable or lock it after initialization since you cannot add more extractors after the index is 'warmed'

        /// <summary>
        /// Gets the list of facet extractors
        /// </summary>
        public List<IFacetExtractor> FacetExtractors { get; private set; }
        
        internal List<IFacetComb> FacetCombs { get; private set; }

        /// <summary>
        /// Returns the FacetMap associated with this configuration
        /// </summary>
        public FacetMap FacetMap { get; private set; }

        /// <summary>
        /// Returns the currently associated IExternalDataProvider if one has been assigned
        /// </summary>
        public IExternalDataProvider ExternalDataProvider { get; private set; }

        //TODO: This is used in ReaderData.ReadFacets, but it's never set to true... when do we want this?
        public bool CacheAllQueryFilters { get; private set; }

        
    }
}
