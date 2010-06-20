using System.Web;
using System.Xml.Linq;

namespace UmbracoExamine.DataServices
{
    public interface INamedService
    {
        XDocument GetAllData(string indexType, string xpath);
    }
}
