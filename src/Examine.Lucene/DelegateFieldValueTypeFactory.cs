using System;
using Examine.Lucene.Indexing;

namespace Examine.Lucene
{
    /// <inheritdoc />
    /// <summary>
    /// A factory to create a <see cref="T:Examine.LuceneEngine.Indexing.IIndexFieldValueType" /> for a field name based on a Func delegate
    /// </summary>
    public class DelegateFieldValueTypeFactory : IFieldValueTypeFactory
    {
        private readonly Func<string, IIndexFieldValueType> _factory;

        /// <inheritdoc/>
        public DelegateFieldValueTypeFactory(Func<string, IIndexFieldValueType> factory)
        {
            _factory = factory;
        }

        /// <inheritdoc/>
        public IIndexFieldValueType Create(string fieldName) => _factory(fieldName);
    }
}
