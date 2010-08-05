using System;
using System.Xml.Linq;
namespace LuceneExamine.DataServices
{
    public interface IMediaService 
    {
        XDocument GetLatestMediaByXpath(string xpath);
    }
}
