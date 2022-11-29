namespace Examine.Web.Demo.Data.Models
{
    public class IndexInformation
    {
        public IndexInformation(long documentCount, List<string> fields)
        {
            DocumentCount = documentCount;
            Fields = fields;
            FieldCount = fields.Count;
        }

        public long DocumentCount { get; }
        public List<string> Fields { get; }
        public int FieldCount { get; set; }
    }
}
