using System;
using System.Web;
using System.Web.Configuration;
using Examine.LuceneEngine.Faceting;

namespace Examine.Web.Demo.Models
{
    public class TestExternalDataProvider : IExternalDataProvider
    {
        public static TestExternalDataProvider Instance = new TestExternalDataProvider();

        
        public object GetData(long id)
        {            
            return new TestExternalData() { Likes = new Random((int)id).Next(0, 100000) };
        }
    }
}