namespace Examine.Search
{
    /// <summary>
    /// Spatial Operation Type
    /// </summary>
    public enum ExamineSpatialOperation
    {
        Intersects = 0,
        Overlaps = 1,
        IsWithin = 2,
        BoundingBoxIntersects = 3,
        BoundingBoxWithin = 4,
        Contains = 5,
        IsDisjointTo = 6,
        IsEqualTo = 7
    }
}
