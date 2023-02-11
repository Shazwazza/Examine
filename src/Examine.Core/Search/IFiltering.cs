using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.Search
{
    public interface IFiltering
    {
        /// <summary>
        /// Executes Spatial operation as a Filter on field and shape
        /// </summary>
        /// <param name="field">Index field name</param>
        /// <param name="shape">Shape</param>
        /// <returns></returns>
        IBooleanOperation SpatialOperationFilter(string field, ExamineSpatialOperation spatialOperation, Func<IExamineSpatialShapeFactory, IExamineSpatialShape> shape);
    }
}
