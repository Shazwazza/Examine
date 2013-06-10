using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.ComponentModel;
using Examine.LuceneEngine.Indexing;
using Lucene.Net.Documents;
using Examine;

namespace Examine.LuceneEngine
{
    /// <summary>
    /// Event arguments for a Document Writing event
    /// </summary>
    public class DocumentWritingEventArgs : CancelEventArgs, INodeEventArgs
    {
	    /// <summary>
	    /// Lucene.NET Document, including all previously added fields
	    /// </summary>        
	    public Document Document
	    {
			[SecuritySafeCritical]
			get;
			[SecuritySafeCritical]
			private set;
	    }

        public ValueSet Values { get; private set; }

        /// <summary>
        /// Fields of the indexer
        /// </summary>
        [Obsolete("Use ValueSet instead")]
        public Dictionary<string, string> Fields { get; private set; }
        
        /// <summary>
        /// NodeId of the document being written
        /// </summary>
        [Obsolete("Use ValueSet instead")]        
        public int NodeId { get; private set; }


        public DocumentWritingEventArgs(Document d, ValueSet values)
        {
            Document = d;
            Values = values;


            //Legacy stuff
            NodeId = (int) values.Id;
            Fields = values.ToLegacyFields();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="d"></param>
        /// <param name="fields"></param>
		[SecuritySafeCritical]
        [Obsolete("Use ValueSet instead")]
        public DocumentWritingEventArgs(int nodeId, Document d, Dictionary<string, string> fields)
            :this(d, ValueSet.FromLegacyFields(nodeId, null, fields))
        {            
        }
    }
}
