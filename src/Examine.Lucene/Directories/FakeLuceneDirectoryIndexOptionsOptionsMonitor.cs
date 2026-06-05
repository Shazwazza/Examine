using System;
using Microsoft.Extensions.Options;

namespace Examine.Lucene.Directories
{
    internal sealed class FakeLuceneDirectoryIndexOptionsOptionsMonitor : IOptionsMonitor<LuceneDirectoryIndexOptions>
    {
        private static readonly LuceneDirectoryIndexOptions Default = new LuceneDirectoryIndexOptions();

        public LuceneDirectoryIndexOptions CurrentValue => Default;

        public LuceneDirectoryIndexOptions Get(string? name) => Default;

        public IDisposable OnChange(Action<LuceneDirectoryIndexOptions, string> listener) => throw new NotImplementedException();
    }
}
