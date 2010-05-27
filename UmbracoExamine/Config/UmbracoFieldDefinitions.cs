using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;

namespace UmbracoExamine.Config
{
    /// <summary>
    /// A class that defines the type of index for each Umbraco field (non user defined fields)
    /// Alot of standard umbraco fields shouldn't be tokenized or even indexed, just stored into lucene
    /// for retreival after searching.
    /// </summary>
    public static class UmbracoFieldPolicies
    {
        private static readonly Dictionary<string, Field.Index> m_Definitions
            = new Dictionary<string, Field.Index>()
            {
                { "id", Field.Index.NOT_ANALYZED},
                { "version", Field.Index.NO},
                { "parentID", Field.Index.NO},
                { "level", Field.Index.NO},
                { "writerID", Field.Index.NO},
                { "creatorID", Field.Index.NO},
                { "nodeType", Field.Index.NOT_ANALYZED},
                { "template", Field.Index.NOT_ANALYZED},
                { "sortOrder", Field.Index.NO},
                { "createDate", Field.Index.NOT_ANALYZED_NO_NORMS},
                { "updateDate", Field.Index.NOT_ANALYZED_NO_NORMS},
                { "nodeName", Field.Index.ANALYZED},
                { "urlName", Field.Index.NOT_ANALYZED},
                { "writerName", Field.Index.NOT_ANALYZED},
                { "creatorName", Field.Index.NOT_ANALYZED},
                { "nodeTypeAlias", Field.Index.NOT_ANALYZED},
                { "path", Field.Index.NOT_ANALYZED}
            };

        public static IEnumerable<KeyValuePair<string, Field.Index>> GetPolicies()
        {
            return m_Definitions;
        }

        /// <summary>
        /// return the index policy for the field name passed in, if not found, return normal
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static Field.Index GetPolicy(string fieldName)
        {
            var def = m_Definitions.Where(x => x.Key == fieldName);
            return (def.Count() == 0 ? Field.Index.ANALYZED : def.Single().Value);
        }
    }
}
