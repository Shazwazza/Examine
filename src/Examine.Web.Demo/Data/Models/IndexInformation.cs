namespace Examine.Web.Demo.Data.Models
{
    public class IndexInformation
    {
        public IndexInformation(long documentCount, List<string> fields, List<string> searchers, List<string> suggesters)
        {
            DocumentCount = documentCount;
            Fields = fields;
            FieldCount = fields.Count;
            Searchers = searchers;
            Suggesters = suggesters;
        }

        public long DocumentCount { get; }
        public List<string> Fields { get; }
        public int FieldCount { get; set; }

        public List<string> Searchers { get; }

        public List<string> Suggesters { get; }
    }
}
