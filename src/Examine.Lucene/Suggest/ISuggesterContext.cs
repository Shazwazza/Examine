using Examine.Lucene.Indexing;

namespace Examine.Lucene.Suggest
{
    public interface ISuggesterContext
    {
        IIndexReaderReference GetIndexReader();
        IIndexFieldValueType GetFieldValueType(string fieldName);
    }
}
