using System;
using Microsoft.Extensions.Options;

namespace Examine.Lucene.Directories
{
    internal sealed class FakeLuceneDirectoryIndexOptionsOptionsMonitor : IOptionsMonitor<LuceneDirectoryIndexOptions>
    {
        private static readonly LuceneDirectoryIndexOptions s_default = new LuceneDirectoryIndexOptions();

        public LuceneDirectoryIndexOptions CurrentValue => s_default;

        public LuceneDirectoryIndexOptions Get(string name) => s_default;

        public IDisposable OnChange(Action<LuceneDirectoryIndexOptions, string> listener) => throw new NotImplementedException();
    }
}
