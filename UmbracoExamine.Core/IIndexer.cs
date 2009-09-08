using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UmbracoExamine.Core
{
    public interface IIndexer
    {

        void ReIndexNode(int nodeId);
        bool DeleteFromIndex(int nodeId);
        void IndexAll();

    }
}
