namespace Examine.LuceneEngine.Faceting
{
    public struct FacetReferenceInfo
    {
        public string FieldName { get; private set; }

        public int Id { get; private set; }

        public FacetReferenceInfo(string facetName, int id) : this()
        {
            FieldName = facetName;
            Id = id;
        }
    }
}