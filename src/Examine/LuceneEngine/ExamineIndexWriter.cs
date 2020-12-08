using Examine.LuceneEngine.Directories;
using Lucene.Net.Analysis;
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
        public ExamineIndexWriter(Directory d, Analyzer a, MaxFieldLength mfl) : base(d, a, mfl)
        {
        }

        public ExamineIndexWriter(Directory d, Analyzer a, bool create, MaxFieldLength mfl) : base(d, a, create, mfl)
        {
        }

        public ExamineIndexWriter(Directory d, Analyzer a, IndexDeletionPolicy deletionPolicy, MaxFieldLength mfl) : base(d, a, deletionPolicy, mfl)
        {
        }

        public ExamineIndexWriter(Directory d, Analyzer a, bool create, IndexDeletionPolicy deletionPolicy, MaxFieldLength mfl) : base(d, a, create, deletionPolicy, mfl)
        {
        }

        public ExamineIndexWriter(Directory d, Analyzer a, IndexDeletionPolicy deletionPolicy, MaxFieldLength mfl, IndexCommit commit) : base(d, a, deletionPolicy, mfl, commit)
        {
        }

        /// <summary>
        /// Calls base.Commit() then Invokes the on commit action
        /// </summary>
        public void ExamineCommit()
        {
            base.Commit();
            if(Directory is ExamineDirectory examineDirectory)
            {
                examineDirectory.InvokeOnCommit(this);
            }
        }
    }
}
