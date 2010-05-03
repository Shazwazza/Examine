using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using Examine;

namespace UmbracoExamine
{
    public class XsltExtensions
    {

        public static XPathNodeIterator Search(string criteria)
        {
            //TODO: Finish all of these methods
            var resuts = ExamineManager.Instance.Search("sdf", true);

            return null;
        }

    }
}
