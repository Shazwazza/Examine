using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine.Faceting;

namespace Examine.LuceneEngine.SearchCriteria
{
    public class DocumentData
    {
        public DocumentData(ReaderData data, int document)
        {
            Data = data;
            SubDocument = document;
        }

        public ReaderData Data { get; set; }
        public int SubDocument { get; set; }
    }
}
