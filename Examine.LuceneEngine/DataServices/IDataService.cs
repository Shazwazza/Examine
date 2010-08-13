using System.Web;


namespace LuceneExamine.DataServices
{
    public interface IDataService
    {
        IContentService ContentService { get; }
        ILogService LogService { get; }
        IMediaService MediaService { get; }

        string MapPath(string virtualPath);
    }
}