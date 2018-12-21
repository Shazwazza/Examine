using Examine.LuceneEngine.Indexing;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// A factory to create a <see cref="IIndexFieldValueType"/> for a field name
    /// </summary>
    public interface IFieldValueTypeFactory
    {
        IIndexFieldValueType Create(string fieldName);
    }
}