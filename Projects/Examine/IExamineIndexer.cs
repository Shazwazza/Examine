using Examine.LuceneEngine.Indexing;

namespace Examine
{
    //TODO: Figure out how to properly remove IIndexer

    /// <summary>
    /// Interface for indexing
    /// </summary>
    public interface IExamineIndexer
    {
        void IndexItems(params ValueSet[] nodes);
        void DeleteFromIndex(string nodeId);
    }
}