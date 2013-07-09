using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;

namespace Examine.LuceneEngine.Faceting
{
    /// <summary>
    /// A facet value and level that can be added to a Lucene document for indexing
    /// </summary>
    public class FacetValue : PayloadDataTokenStream
    {
        public string Value { get; private set; }
        public float Level { get; private set; }

        public FacetValue(string value, float level = .5f)
            : base(value)
        {
            Value = value;
            Level = level;
            SetValue(level);
        }      
    }
}
