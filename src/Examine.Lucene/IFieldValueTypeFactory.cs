using Examine.Lucene.Indexing;

namespace Examine.Lucene
{
    /// <summary>
    /// A factory to create a <see cref="IIndexFieldValueType"/> for a field name
    /// </summary>
    public interface IFieldValueTypeFactory
    {
        /// <summary>
        /// Creates a <see cref="IIndexFieldValueType"/> for a field name
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        IIndexFieldValueType Create(string fieldName);
    }
}
