using System;
using System.Collections.Generic;
using System.Text;
using Examine.Lucene.Search;

namespace Examine.Search
{
    public readonly struct FacetValue : IFacetValue
    {
        public string Label { get; }

        public float Value { get; }

        public FacetValue(string label, float value)
        {
            Label = label;
            Value = value;
        }
    }
}
