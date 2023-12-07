using Examine.Search;

namespace Examine.Lucene.Spatial.Search
{
    /// <summary>
    /// Empty Spatial Shape
    /// </summary>
    public class ExamineLuceneEmptyShape : ExamineLuceneShape, IExamineSpatialEmptyShape
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ExamineLuceneEmptyShape() : base(null)
        {
        }

        /// <inheritdoc/>
        public override bool IsEmpty => true;
    }
}
