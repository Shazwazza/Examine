using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Search
{
    public interface IExamineSpatialShapeFactory
    {
        IExamineSpatialPoint CreatePoint(double latitude, double longitude);

        IExamineSpatialRectangle CreateRectangle(double minX, double maxX, double minY, double maxY);

        /// <summary>
        /// Creates a Empty Shape. Used for not exists
        /// </summary>
        /// <returns></returns>
        IExamineSpatialEmptyShape CreateEmpty();

        IExamineSpatialCircle CreateCircle(double x, double y, double distance);

        /// <summary>
        /// Create a Circle around a point on a spherical Earth model (Mean Earth Radius in Kilometers)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        IExamineSpatialCircle CreateEarthMeanSearchRadiusKMCircle(double x, double y, double radius);

        /// <summary>
        /// Create a Circle around a point on a spherical Earth model (Equatorial Earth Radius in Kilometers)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        IExamineSpatialCircle CreateEarthEquatorialSearchRadiusKMCircle(double x, double y, double radius);
    }
}
