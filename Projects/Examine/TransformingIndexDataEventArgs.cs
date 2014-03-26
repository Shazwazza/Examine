using System;
using System.Collections.Generic;
using Examine.LuceneEngine.Indexing;

namespace Examine
{
    /// <summary>
    /// Aguments for the TransformIndexValues event
    /// </summary>
    public class TransformingIndexDataEventArgs : EventArgs
    {
        public ValueSet ValueSet { get; private set; }
        public IDictionary<string, IEnumerable<object>> OriginalValues { get; private set; }

        public TransformingIndexDataEventArgs(ValueSet valueSet, IDictionary<string, IEnumerable<object>> originalValues)
        {
            ValueSet = valueSet;
            OriginalValues = originalValues;
        }
    }
}