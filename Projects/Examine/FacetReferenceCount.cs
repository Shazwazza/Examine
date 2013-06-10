namespace Examine
{
    public class FacetReferenceCount
    {
        public string FieldName { get; private set; }

        public int Count { get; private set; }        

        public FacetReferenceCount(string fieldName, int count)
        {           
            FieldName = fieldName;
            Count = count;
        }
    }
}