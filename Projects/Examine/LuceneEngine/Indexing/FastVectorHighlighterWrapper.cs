using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Vectorhighlight;
using Examine.LuceneEngine.SearchCriteria;

namespace Examine.LuceneEngine.Indexing
{
    public class FastVectorHighlighterWrapper : IHighlighter
    {
        private readonly IIndexValueType _valueType;
        private readonly Searcher _searcher;
        private FastVectorHighlighter _fvh;
        private FieldQuery _fq;

        public FastVectorHighlighterWrapper(Query query, IIndexValueType valueType, Searcher searcher)
        {

            _valueType = valueType;
            _searcher = searcher;
            _fvh = new FastVectorHighlighter(FastVectorHighlighter.DEFAULT_PHRASE_HIGHLIGHT, FastVectorHighlighter.DEFAULT_FIELD_MATCH,
                    new SimpleFragListBuilder(), new SimpleFragmentsBuilder(new []{"<em>"}, new []{"</em>"}));

            _fq = _fvh.GetFieldQuery(query);

        }
        public string Highlight(int docId)
        {
            var reader = _searcher.GetSubSearchers().FirstOrDefault();
            if (reader != null)
            {
                return _fvh.GetBestFragment(_fq, reader.GetIndexReader(), docId, _valueType.FieldName, 300);
            }

            return null;
        }
    }
}
