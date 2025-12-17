using System.Collections.Generic;
using System.Linq;
using Examine.Search;
using Spatial4n.Context;
using Spatial4n.Distance;
using Spatial4n.Shapes;

namespace Examine.Lucene.Spatial.Search
{
    public class Spatial4nShapeFactory : ISpatialShapeFactory
    {
        private SpatialContext _spatialContext;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="spatialContext">Spatial Context</param>
        public Spatial4nShapeFactory(SpatialContext spatialContext)
        {
            _spatialContext = spatialContext;
        }

        public SpatialContext SpatialContext { get => _spatialContext; }

        /// <inheritdoc/>
        public ISpatialCircle CreateCircle(double x, double y, double distance)
        {
            var spatial4NCircle = _spatialContext.MakeCircle(x, y, distance);
            return new ExamineLuceneCircle(spatial4NCircle);
        }

        /// <inheritdoc/>
        public ISpatialCircle CreateEarthEquatorialSearchRadiusKMCircle(double x, double y, double radius)
        {
            var spatial4NCircle = _spatialContext.MakeCircle(x, y, DistanceUtils.Dist2Degrees(radius, DistanceUtils.EarthEquatorialRadiusKilometers));
            return new ExamineLuceneCircle(spatial4NCircle);
        }

        /// <inheritdoc/>
        public ISpatialCircle CreateEarthMeanSearchRadiusKMCircle(double x, double y, double radius)
        {
            var spatial4NCircle = _spatialContext.MakeCircle(x, y, DistanceUtils.Dist2Degrees(radius, DistanceUtils.EarthMeanRadiusKilometers));
            return new ExamineLuceneCircle(spatial4NCircle);
        }

        /// <inheritdoc/>
        public ISpatialEmptyShape CreateEmpty()
        {
            return new ExamineLuceneEmptyShape();
        }

        /// <inheritdoc/>
        public ISpatialPoint CreateGeoPoint(double latitude, double longitude)
        {
            //Swapped on purpose
            double y = latitude;
            double x = longitude;
            var spatial4NPoint = _spatialContext.MakePoint(x, y);
            return new ExamineLucenePoint(spatial4NPoint);
        }

        /// <inheritdoc/>
        public ISpatialPoint CreatePoint(double x, double y)
        {
            var spatial4NPoint = _spatialContext.MakePoint(x, y);
            return new ExamineLucenePoint(spatial4NPoint);
        }

        /// <inheritdoc/>
        public ISpatialRectangle CreateRectangle(double minX, double maxX, double minY, double maxY)
        {
            var spatial4NRect = _spatialContext.MakeRectangle(minX, maxX, minY, maxY);
            return new ExamineLuceneRectangle(spatial4NRect);
        }

        /// <inheritdoc/>
        public ISpatialShapeCollection CreateShapeCollection(IList<ISpatialShape> shapes)
        {
            var shapeList = shapes.Select(x => x as ExamineLuceneShape).Select(x => x.Shape).ToList();
            var shapeCollection = new ShapeCollection(shapeList, SpatialContext);
            var examineShapeCollection = new ExamineLuceneShapeCollection(shapeCollection);
            return examineShapeCollection;
        }

        /// <inheritdoc/>
        public ISpatialLineString CreateLineString(IList<ISpatialPoint> points)
        {
            var shapeList = points.Select(x => x as ExamineLucenePoint).Select(x => x.Shape as IPoint).ToList();
            var spatial4NRect = _spatialContext.MakeLineString(shapeList);
            return new ExamineLuceneLineString(spatial4NRect);
        }
    }
}
