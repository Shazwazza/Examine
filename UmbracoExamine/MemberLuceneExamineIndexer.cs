using System.Collections;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using umbraco.cms.businesslogic.member;

namespace UmbracoExamine
{
    public class MemberLuceneExamineIndexer : LuceneExamineIndexer
    {
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

        private XElement GetMemberItem(int nodeId)
        {
            var nodes = umbraco.library.GetMember(nodeId);
            return XElement.Parse(nodes.Current.OuterXml);
        }
    }
}
