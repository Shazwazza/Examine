using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UmbracoExamine.DataServices;

namespace Examine.Test.DataServices
{
    public class TestDataService : IDataService
    {

        public TestDataService()
        {
            ContentService = new TestContentService();
            LogService = new TestLogService();
            MediaService = new TestMediaService();
        }

        #region IDataService Members

        public IContentService ContentService { get; private set; }

        public ILogService LogService { get; private set; }

        public IMediaService MediaService { get; private set; }

        #endregion
    
    
    }
    
   
    
    

    

}
