using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Examine.LuceneEngine.Indexing
{
    public interface IHighlighter
    {
        string Highlight(int docId);
    }
}
