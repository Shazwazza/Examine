using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UmbracoExamine.Core
{
    public interface IIndexer
    {

        void ReIndexNode(int nodeId, IndexType type);
        void DeleteFromIndex(int nodeId);
        void IndexAll(IndexType type);
        void RebuildIndex();

        IIndexCriteria IndexerData { get; set; }

    }
}
