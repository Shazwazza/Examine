using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Documents;

namespace UmbracoExamine.Providers.Config
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
                { "id", Field.Index.UN_TOKENIZED},
                { "version", Field.Index.NO},
                { "parentID", Field.Index.NO},
                { "level", Field.Index.NO},
                { "writerID", Field.Index.NO},
                { "creatorID", Field.Index.NO},
                { "nodeType", Field.Index.UN_TOKENIZED},
                { "template", Field.Index.UN_TOKENIZED},
                { "sortOrder", Field.Index.NO},
                { "createDate", Field.Index.NO_NORMS},
                { "updateDate", Field.Index.NO_NORMS},
                { "nodeName", Field.Index.TOKENIZED},
                { "urlName", Field.Index.UN_TOKENIZED},
                { "writerName", Field.Index.UN_TOKENIZED},
                { "creatorName", Field.Index.UN_TOKENIZED},
                { "nodeTypeAlias", Field.Index.UN_TOKENIZED},
                { "path", Field.Index.NO}
            };

        /// <summary>
        /// return the index policy for the field name passed in, if not found, return normal
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static Field.Index GetPolicy(string fieldName)
        {
            var def = m_Definitions.Where(x => x.Key == fieldName);
            return (def.Count() == 0 ? Field.Index.TOKENIZED : def.Single().Value);
        }
    }
}
