using System;
using Examine.LuceneEngine.Indexing;

namespace Examine.LuceneEngine
{
    /// <inheritdoc />
    /// <summary>
    /// A factory to create a <see cref="T:Examine.LuceneEngine.Indexing.IIndexValueType" /> for a field name based on a Func delegate
    /// </summary>
    public class DelegateFieldValueTypeFactory : IFieldValueTypeFactory
    {
        private readonly Func<string, IIndexValueType> _factory;

        public DelegateFieldValueTypeFactory(Func<string, IIndexValueType> factory)
        {
            _factory = factory;
        }

        public IIndexValueType Create(string fieldName)
        {
            return _factory(fieldName);
        }
    }
}