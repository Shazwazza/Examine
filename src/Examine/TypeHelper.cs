using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace Examine
{
    internal static class TypeHelper
    {
        /// <summary>
        /// Find a type by name
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <remarks>
        /// Takes into account if this is an aspnet hosted application
        /// </remarks>
        public static Type FindType(string typeName)
        {
         //  todo: replace  var isHosted = HttpContext.Current != null || HostingEnvironment.IsHosted; isHosted ? BuildManager.GetType(typeName, false) :

            return  Type.GetType(typeName);
        }
    }
}
