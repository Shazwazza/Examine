namespace Examine.LuceneEngine.Faceting
{
    public class DocumentFacet
    {
        public int DocumentId { get; set; }

        public FacetKey Key { get; set; }

        public float Level { get; set; }
    }
}