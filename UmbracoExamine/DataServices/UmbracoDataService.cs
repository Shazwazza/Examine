using System.Web;

namespace UmbracoExamine.DataServices
{
    public class UmbracoDataService : IDataService
    {
        public UmbracoDataService()
        {
            ContentService = new UmbracoContentService();
            MediaService = new UmbracoMediaService();
            LogService = new UmbracoLogService();
            HttpContext = new HttpContextWrapper(System.Web.HttpContext.Current);
        }

        public IContentService ContentService { get; private set; }
        public IMediaService MediaService { get; private set; }
        public ILogService LogService { get; private set; }
        public HttpContextBase HttpContext { get; private set; }
    }
}
