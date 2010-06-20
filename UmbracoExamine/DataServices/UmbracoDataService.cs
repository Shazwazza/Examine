using System.Web;
using System.Web.Hosting;

namespace UmbracoExamine.DataServices
{
    public class UmbracoDataService : IDataService
    {
        public UmbracoDataService()
        {
            ContentService = new UmbracoContentService();
            MediaService = new UmbracoMediaService();
            LogService = new UmbracoLogService();
            NamedService = new NamedService();
        }

        public IContentService ContentService { get; private set; }
        public IMediaService MediaService { get; private set; }
        public ILogService LogService { get; private set; }
        public INamedService NamedService { get; private set; }

        public string MapPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }

    }
}
