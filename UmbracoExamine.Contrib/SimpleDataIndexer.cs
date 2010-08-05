using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine.LuceneEngine;
using Examine;
using Examine.LuceneEngine.Config;
using System.Xml.Linq;

namespace UmbracoExamine.Contrib
{
    public class SimpleDataIndexer : LuceneExamineIndexer
    {


        protected override void PerformIndexAll(string type)
        {
            
        }

        protected override IIndexCriteria GetIndexerData(IndexSet indexSet)
        {
            return new IndexCriteria(
                indexSet.IndexAttributeFields.ToList().Select(x => x.Name).ToArray(),
                indexSet.IndexUserFields.ToList().Select(x => x.Name).ToArray(),
                indexSet.IncludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                indexSet.ExcludeNodeTypes.ToList().Select(x => x.Name).ToArray(),
                indexSet.IndexParentId);
        }

        protected override void PerformIndexRebuild()
        {
            //IndexAll(IndexTypes.Content);
            //IndexAll(IndexTypes.Media);   
        }

        /// <summary>
        /// Translates the XElement structure into a dictionary object to be indexed
        /// </summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected override Dictionary<string, string> GetDataToIndex(XElement node, string type)
        {
            return new Dictionary<string, string>();
        }



        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);


        }

    }
}
