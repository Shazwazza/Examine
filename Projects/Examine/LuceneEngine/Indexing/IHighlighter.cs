using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;

namespace Examine.LuceneEngine.Indexing
{
    public interface IHighlighter
    {
        string Highlight(Document document);
    }
}
