using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Search
{
    /// <summary>
    /// Creates Shapes
    /// </summary>
    public interface ISpatialShapeFactory
    {
        /// <summary>
        /// Create a Point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        ISpatialPoint CreatePoint(double x, double y);

        /// <summary>
        /// Create a Point from a Latitude and longitude
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        ISpatialPoint CreateGeoPoint(double latitude, double longitude);

        /// <summary>
        /// Create a Rectangle.
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        /// <returns></returns>
        ISpatialRectangle CreateRectangle(double minX, double maxX, double minY, double maxY);

        /// <summary>
        /// Creates a Empty Shape. Used for not exists
        /// </summary>
        /// <returns></returns>
        ISpatialEmptyShape CreateEmpty();

        /// <summary>
        /// Create a circle
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        ISpatialCircle CreateCircle(double x, double y, double distance);

        /// <summary>
        /// Create a Circle around a point on a spherical Earth model (Mean Earth Radius in Kilometers)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        ISpatialCircle CreateEarthMeanSearchRadiusKMCircle(double x, double y, double radius);

        /// <summary>
        /// Create a Circle around a point on a spherical Earth model (Equatorial Earth Radius in Kilometers)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        ISpatialCircle CreateEarthEquatorialSearchRadiusKMCircle(double x, double y, double radius);

        /// <summary>
        /// Create a Line String from a ordered set of Points (Vertices)
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        ISpatialLineString CreateLineString(IList<ISpatialPoint> points);

        /// <summary>
        /// Create a Shape Collection from a list of Shapes
        /// </summary>
        /// <param name="shapes"></param>
        /// <returns></returns>
        ISpatialShapeCollection CreateShapeCollection(IList<ISpatialShape> shapes);
    }
}
