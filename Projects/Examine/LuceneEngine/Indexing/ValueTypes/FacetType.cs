using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;

namespace Examine.LuceneEngine.Indexing.ValueTypes
{
    public class FacetType : RawStringType
    {
        private readonly Func<IFacetExtractor> _extractorFactory;

        public FacetType(string fieldName, bool store = true,
            Func<IFacetExtractor> extractorFactory = null) : base(fieldName, store)
        {
            _extractorFactory = extractorFactory ?? (()=>new TermFacetExtractor(fieldName));
        }

        public override IFacetExtractor CreateFacetExtractor()
        {
            return _extractorFactory();
        }
    }
}
