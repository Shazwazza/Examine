using System;
using System.Drawing;
using Examine;
using Examine.Providers;

namespace UmbracoExamine.Test.TESTING
{
    public partial class TestProviders : TestControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void TestProviderButton_Click(object sender, EventArgs e)
        {
            foreach (BaseIndexProvider indexer in ExamineManager.Instance.IndexProviderCollection)
            {
                AddTrace("TestProviders", "INDEXER: " + indexer.Name + " : " + indexer.Description, Color.Black);
            }
            foreach (BaseSearchProvider searcher in ExamineManager.Instance.SearchProviderCollection)
            {
                AddTrace("TestProviders", "SEARCHER: " + searcher.Name + " : " + searcher.Description, Color.Black);
            }            

        }
    }
}