using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using umbraco.cms.businesslogic;
using System.Xml.Linq;

namespace UmbracoExamine.Core
{
    public interface IIndexer
    {

        void ReIndexNode(XElement node, IndexType type);
        void DeleteFromIndex(XElement node);
        
        /// <summary>
        /// Re-indexes all data for the index type specified
        /// </summary>
        /// <param name="type"></param>
        void IndexAll(IndexType type);

        /// <summary>
        /// Rebuilds the entire index from scratch for all index types
        /// </summary>
        void RebuildIndex();

        IIndexCriteria IndexerData { get; set; }

    }
}
