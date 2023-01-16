using System;
using Lucene.Net.Index;

namespace Examine.Lucene.Suggest
{
    public interface IIndexReaderReference : IDisposable
    {
        DirectoryReader IndexReader { get; }
    }
}
