namespace Examine.Search
{
    public interface IExamineSpatialShape
    {
        IExamineSpatialPoint Center { get; }
        bool IsEmpty { get; }
    }
}
