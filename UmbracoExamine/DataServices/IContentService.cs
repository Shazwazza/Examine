using System;
using System.Xml.Linq;
namespace UmbracoExamine.DataServices
{
    public interface IContentService
    {
        XDocument GetLatestContentByXPath(string xpath);
        XDocument GetPublishedContentByXPath(string xpath);
        string StripHtml(string value);
        bool IsProtected(int nodeId, string path);
    }
}
