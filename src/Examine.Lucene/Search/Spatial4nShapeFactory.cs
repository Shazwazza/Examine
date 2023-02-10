using Examine.Search;
using Spatial4n.Context;
using Spatial4n.Distance;

namespace Examine.Lucene.Search
{
    public class Spatial4nShapeFactory : IExamineSpatialShapeFactory
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
        public IExamineSpatialCircle CreateCircle(double x, double y, double distance)
        {
            var spatial4NCircle = _spatialContext.MakeCircle(x, y, distance);
            return new ExamineLuceneCircle(spatial4NCircle);
        }

        public IExamineSpatialCircle CreateEarthEquatorialSearchRadiusKMCircle(double x, double y, double radius)
        {
            var spatial4NCircle = _spatialContext.MakeCircle(x, y, DistanceUtils.Dist2Degrees(radius, DistanceUtils.EarthEquatorialRadiusKilometers));
            return new ExamineLuceneCircle(spatial4NCircle);
        }
        public IExamineSpatialCircle CreateEarthMeanSearchRadiusKMCircle(double x, double y, double radius)
        {
            var spatial4NCircle = _spatialContext.MakeCircle(x, y, DistanceUtils.Dist2Degrees(radius, DistanceUtils.EarthMeanRadiusKilometers));
            return new ExamineLuceneCircle(spatial4NCircle);
        }
        public IExamineSpatialEmptyShape CreateEmpty()
        {
            return new ExamineLuceneEmptyShape();
        }

        /// <summary>
        /// Point
        /// </summary>
        /// <param name="x">x or latitude</param>
        /// <param name="y">y or longitude</param>
        /// <returns></returns>
        public IExamineSpatialPoint CreatePoint(double x, double y)
        {
            var spatial4NPoint = _spatialContext.MakePoint(x, y);
            return new ExamineLucenePoint(spatial4NPoint);
        }
        public IExamineSpatialRectangle CreateRectangle(double minX, double maxX, double minY, double maxY)
        {
            var spatial4NRect = _spatialContext.MakeRectangle(minX, maxX, minY, maxY);
            return new ExamineLuceneRectangle(spatial4NRect);
        }
    }
}
