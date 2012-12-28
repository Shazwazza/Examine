using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Index;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Extension methods for Lucene
    /// </summary>
    public static class LuceneExtensions
    {
		[SecuritySafeCritical]
        public static ReaderStatus GetReaderStatus(this IndexSearcher searcher)
        {
            return searcher.GetIndexReader().GetReaderStatus();
        }        

		[SecuritySafeCritical]
        public static ReaderStatus GetReaderStatus(this IndexReader reader)
        {
            ReaderStatus status = ReaderStatus.NotCurrent;
            try
            {
                status = reader.IsCurrent() ? ReaderStatus.Current : ReaderStatus.NotCurrent;
            }
            catch (AlreadyClosedException)
            {
                status = ReaderStatus.Closed;
            }
            return status;
        }

    }
}
