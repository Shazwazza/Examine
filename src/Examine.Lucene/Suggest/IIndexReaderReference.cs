using System;
using Lucene.Net.Index;

namespace Examine.Lucene.Suggest
{
    /// <summary>
    /// Reference to an instance of an IndexReader on a Lucene Index
    /// </summary>
    public interface IIndexReaderReference : IDisposable
    {
        /// <summary>
        /// Index Reader
        /// </summary>
        DirectoryReader IndexReader { get; }
    }
}
