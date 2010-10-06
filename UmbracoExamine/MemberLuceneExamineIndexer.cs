using System.Collections;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using umbraco.cms.businesslogic.member;
using Examine.LuceneEngine;
using System.Collections.Generic;

namespace UmbracoExamine
{
    /// <summary>
    /// 
    /// </summary>
    public class MemberLuceneExamineIndexer : UmbracoExamineIndexer
    {
        protected override IEnumerable<string> SupportedTypes
        {
            get
            {
                return new string[] { IndexTypes.Member };
            }
        }

        protected override XDocument GetXDocument(string xPath, string type)
        {
            if (type == IndexTypes.Member)
            {
                Member[] rootMembers = Member.GetAll;
                var xmlMember = XDocument.Parse("<member></member>");
                foreach (Member member in rootMembers)
                {
                    xmlMember.Root.Add(GetMemberItem(member.Id));
                }
                var result = ((IEnumerable)xmlMember.XPathEvaluate(xPath)).Cast<XElement>();
                return result.ToXDocument(); 
            }

            return null;
        }

        protected override System.Collections.Generic.Dictionary<string, string> GetDataToIndex(XElement node, string type)
        {
            var data = base.GetDataToIndex(node, type);

            if (data.ContainsKey("email"))
            {
                data.Add("__email",data["email"].Replace("."," ").Replace("@"," "));
            }

            return data;
        }

        private XElement GetMemberItem(int nodeId)
        {
            var nodes = umbraco.library.GetMember(nodeId);
            return XElement.Parse(nodes.Current.OuterXml);
        }
    }
}
