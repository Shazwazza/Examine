using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UmbracoExamine.DataServices
{
    public class UmbracoDataService : UmbracoExamine.DataServices.IDataService
    {
        public UmbracoDataService()
        {
            ContentService = new UmbracoContentService();
            MediaService = new UmbracoMediaService();
            LogService = new UmbracoLogService();
        }

        public IContentService ContentService { get; private set; }
        public IMediaService MediaService { get; private set; }
        public ILogService LogService { get; private set; }
        

    }
}
