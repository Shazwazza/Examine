using System;
namespace UmbracoExamine.DataServices
{
    public interface IDataService
    {
        IContentService ContentService { get; }
        ILogService LogService { get; }
        IMediaService MediaService { get; }
    }
}
