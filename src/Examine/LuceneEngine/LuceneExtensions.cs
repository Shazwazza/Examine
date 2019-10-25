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
        /// <summary>
        /// Copies from IndexInput to IndexOutput
        /// </summary>
        /// <param name="indexInput"></param>
        /// <param name="indexOutput"></param>
        /// <param name="name"></param>
        /// <remarks>
        /// From Another interesting project I found: 
        /// http://www.compass-project.org/
        /// which has some interesting bits like:
        /// https://github.com/kimchy/compass/blob/master/src/main/src/org/apache/lucene/index/LuceneUtils.java
        /// 
        /// </remarks>
        
        internal static void CopyTo(this IndexInput indexInput, IndexOutput indexOutput, string name)
        {
            var buffer = new byte[32768];

            long length = indexInput.Length;
            long remainder = length;
            int chunk = buffer.Length;

            while (remainder > 0)
            {
                int len = (int)Math.Min(chunk, remainder);
                indexInput.ReadBytes(buffer, 0, len);
                indexOutput.WriteBytes(buffer, len);
                remainder -= len;
            }

            // Verify that remainder is 0
            if (remainder != 0)
                throw new InvalidOperationException(
                        "Non-zero remainder length after copying [" + remainder
                                + "] (id [" + name + "] length [" + length
                                + "] buffer size [" + chunk + "])");
        }


        
        public static ReaderStatus GetReaderStatus(this IndexSearcher searcher)
        {
            return searcher.IndexReader.GetReaderStatus();
        }        

		
        public static ReaderStatus GetReaderStatus(this IndexReader reader)
        {
            ReaderStatus status = ReaderStatus.NotCurrent;
            try
            {
                status =  ReaderStatus.Current;//todo: fix checking of status
            }
            catch (ObjectDisposedException)
            {
                status = ReaderStatus.Closed;
            }
            return status;
        }

    }
}
