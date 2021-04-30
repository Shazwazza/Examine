using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.ComponentModel;
using Lucene.Net.Documents;
using Examine;

namespace Examine.Lucene
{
    /// <summary>
    /// Event arguments for a Document Writing event
    /// </summary>
    public class DocumentWritingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Lucene.NET Document, including all previously added fields
        /// </summary>        
        public Document Document { get; }

        /// <summary>
        /// Fields of the indexer
        /// </summary>
        public ValueSet ValueSet { get; }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueSet"></param>
        /// <param name="d"></param>
        public DocumentWritingEventArgs(ValueSet valueSet, Document d)
        {
            this.Document = d;
            this.ValueSet = valueSet;
        }
    }
}
