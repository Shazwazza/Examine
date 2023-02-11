using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Spatial4n.Shapes;

namespace Examine.Lucene.Search
{
    public class ExamineLuceneShapeCollection : ExamineLuceneShape, IExamineSpatialShapeCollection
    {
        public ExamineLuceneShapeCollection(ShapeCollection shapes) : base(null)
        {
            Shapes = shapes;
        }
        public override bool IsEmpty => Shapes.IsEmpty;

        public ShapeCollection Shapes { get; }

        IList<IExamineSpatialShape> IExamineSpatialShapeCollection.Shapes => Shapes.Shapes.Select(x=> new ExamineLuceneShape(x)).ToList<IExamineSpatialShape>();
    }
}
