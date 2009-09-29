using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using umbraco.cms.businesslogic;

namespace UmbracoExamine.Core
{
    public interface IIndexer
    {

        void ReIndexNode(Content node, IndexType type);
        void DeleteFromIndex(Content node);
        void IndexAll(IndexType type);
        void RebuildIndex();

        IIndexCriteria IndexerData { get; set; }

    }
}
