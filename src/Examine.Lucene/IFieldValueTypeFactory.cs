using Examine.Lucene.Indexing;

namespace Examine.Lucene
{
    /// <summary>
    /// A factory to create a <see cref="IIndexFieldValueType"/> for a field name
    /// </summary>
    public interface IFieldValueTypeFactory
    {
        IIndexFieldValueType Create(string fieldName);
    }
}