using Examine.LuceneEngine.Indexing;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// A factory to create a <see cref="IIndexValueType"/> for a field name
    /// </summary>
    public interface IFieldValueTypeFactory
    {
        IIndexValueType Create(string fieldName);
    }
}