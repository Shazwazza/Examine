using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections;
using umbraco.cms.businesslogic.member;

namespace UmbracoExamine.DataServices
{
    public class NamedService : INamedService
    {
        #region INamedService Members

        public XDocument GetAllData(string indexType, string xpath)
        {
            switch (indexType)
            {
                case "member":
                    //Lookup the member data
                    break;

                default:
                    return UnhandledIndexType(indexType);
            }

            throw new NotImplementedException();
        }

        protected virtual XDocument UnhandledIndexType(string indexType)
        {
            return null;
        }


        /// <summary>
        /// This is quite an intensive operation...
        /// get all root media, then get the XML structure for all children,
        /// then run xpath against the navigator that's created
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns></returns>
        private XDocument GetLatestMemberByXpath(string xpath)
        {

            Member[] rootMembers = Member.GetAll;
            var xmlMember = XDocument.Parse("<member></member>");
            foreach (Member member in rootMembers)
            {
                xmlMember.Root.Add(GetMemberItem(member.Id));
            }
            var result = ((IEnumerable)xmlMember.XPathEvaluate(xpath)).Cast<XElement>();
            return result.ToXDocument();
        }

        private XElement GetMemberItem(int nodeId)
        {
            var nodes = umbraco.library.GetMember(nodeId);
            return XElement.Parse(nodes.Current.OuterXml);
        }

        #endregion
    }
}
