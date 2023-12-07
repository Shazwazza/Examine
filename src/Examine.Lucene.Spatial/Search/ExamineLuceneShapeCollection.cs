using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Spatial.Search
{
    /// <summary>
    /// Collection of Shapes
    /// </summary>
    public class ExamineLuceneShapeCollection : ExamineLuceneShape, IExamineSpatialShapeCollection
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shapes">Shapes</param>
        public ExamineLuceneShapeCollection(ShapeCollection shapes) : base(null)
        {
            Shapes = shapes;
        }

        /// <inheritdoc/>
        public override bool IsEmpty => Shapes.IsEmpty;

        /// <inheritdoc/>
        public ShapeCollection Shapes { get; }

        /// <inheritdoc/>
        IList<IExamineSpatialShape> IExamineSpatialShapeCollection.Shapes => Shapes.Shapes.Select(x=> new ExamineLuceneShape(x)).ToList<IExamineSpatialShape>();
    }
}
