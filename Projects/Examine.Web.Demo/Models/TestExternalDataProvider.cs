using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Examine.LuceneEngine.Faceting;

namespace Examine.Web.Demo.Models
{
    public class TestExternalDataProvider : IExternalDataProvider
    {
        public static TestExternalDataProvider Instance = new TestExternalDataProvider();

        
        public object GetData(long id)
        {
            return new TestExternalData() { Likes = new Random((int)id).Next(0, 1000) };
        }
    }

    public class TestExternalData
    {
        public int Likes { get; set; }
    }
}