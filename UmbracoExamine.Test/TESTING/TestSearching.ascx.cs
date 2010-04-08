using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UmbracoExamine;
using Examine;
using System.Drawing;
using UmbracoExamine.SearchCriteria;
using Examine.SearchCriteria;

namespace UmbracoExamine.Test.TESTING
{
    public partial class TestSearching : TestControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void TestSearch_Click(object sender, EventArgs e)
        {
            //var results = ExamineManager.Instance.Search(SearchTextBox.Text, 100, true);

            var s = "AEIOU";

            var fields = new string[s.Length];
            var query = new string[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                fields[i] = "bodyText";
                query[i] = s[i].ToString();
            }

            var searchCriteria = ExamineManager.Instance.CreateSearchCriteria(IndexType.Content);
            searchCriteria = searchCriteria
                //.NodeTypeAlias("cws".MultipleCharacterWildcard())
                //.And()
                .GroupedOr(fields, query)
                //.And()
                //.GroupedFlexible(new [] { "bodyText", "headerText" }, new[] { BooleanOperation.Not, BooleanOperation.And }, "sucks", "rocks")
                .Compile();

            var results = ExamineManager.Instance.Search(searchCriteria);

            foreach (var r in results)
            {
                AddTrace("Result", string.Format("Score: {0}, NodeId: {1}", r.Score, r.Id), Color.Black);
            }

        }
    }
}