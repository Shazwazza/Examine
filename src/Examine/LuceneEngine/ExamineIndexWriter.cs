using Examine.LuceneEngine.Directories;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examine.LuceneEngine
{
    public class ExamineIndexWriter : IndexWriter
    {
        private ExamineDirectory _examineDirectory;
        public ExamineIndexWriter(Directory d, Analyzer a, MaxFieldLength mfl) : base(d, a, mfl)
        {
            _examineDirectory = d as ExamineDirectory;
        }

        public ExamineIndexWriter(Directory d, Analyzer a, bool create, MaxFieldLength mfl) : base(d, a, create, mfl)
        {
            _examineDirectory = d as ExamineDirectory;
        }

        public ExamineIndexWriter(Directory d, Analyzer a, IndexDeletionPolicy deletionPolicy, MaxFieldLength mfl) : base(d, a, deletionPolicy, mfl)
        {
            _examineDirectory = d as ExamineDirectory;
        }

        public ExamineIndexWriter(Directory d, Analyzer a, bool create, IndexDeletionPolicy deletionPolicy, MaxFieldLength mfl) : base(d, a, create, deletionPolicy, mfl)
        {
            _examineDirectory = d as ExamineDirectory;
        }

        public ExamineIndexWriter(Directory d, Analyzer a, IndexDeletionPolicy deletionPolicy, MaxFieldLength mfl, IndexCommit commit) : base(d, a, deletionPolicy, mfl, commit)
        {
            _examineDirectory = d as ExamineDirectory;
        }

        /// <summary>
        /// Calls base.Commit() then Invokes the on commit action
        /// </summary>
        public void ExamineCommit()
        {
            base.Commit();
            if(_examineDirectory != null)
            {
                _examineDirectory.InvokeOnCommit(this);
            }
        }

        public override void UpdateDocument(Term term, Document doc)
        {
            if(_examineDirectory != null && _examineDirectory.IsReadOnly)
            {
                //Cancel as directory is read only
                return;
            }
            base.UpdateDocument(term, doc);
        }

        public override void AddDocument(Document doc)
        {
            if (_examineDirectory != null && _examineDirectory.IsReadOnly)
            {
                //Cancel as directory is read only
                return;
            }
            base.AddDocument(doc);
        }

        public override void AddDocument(Document doc, Analyzer analyzer)
        {
            if (_examineDirectory != null && _examineDirectory.IsReadOnly)
            {
                //Cancel as directory is read only
                return;
            }
            base.AddDocument(doc, analyzer);
        }
    }
}
