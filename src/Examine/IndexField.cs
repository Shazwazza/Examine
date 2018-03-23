namespace Examine
{
    public class IndexField : IIndexField
    {
        public string Name { get; set; }

        public bool EnableSorting { get; set; }

        public string Type { get; set; }
    }
}