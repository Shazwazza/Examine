using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Search
{
    public interface IExamineSpatialShape
    {
        IExamineSpatialPoint Center { get; }
        bool IsEmpty { get; }
    }

    public interface IExamineSpatialEmptyShape : IExamineSpatialShape
    {

    }

    public interface IExamineSpatialRectangle : IExamineSpatialShape
    {
        /// <summary>The left edge of the X coordinate.</summary>
        double MinX { get; }

        /// <summary>The bottom edge of the Y coordinate.</summary>
        double MinY { get; }

        /// <summary>The right edge of the X coordinate.</summary>
        double MaxX { get; }

        /// <summary>The top edge of the Y coordinate.</summary>
        double MaxY { get; }
    }
    public interface IExamineSpatialCircle : IExamineSpatialShape
    {
        double Radius { get; }
    }
}
