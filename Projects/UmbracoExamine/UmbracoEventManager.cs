using System;
using System.IO;
using umbraco.BusinessLogic;

namespace UmbracoExamine
{
    /// <summary>
    /// An <see cref="umbraco.BusinessLogic.ApplicationBase"/> instance for wiring up Examine to the Umbraco events system
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in upcoming versions")]
    public class UmbracoEventManager : ApplicationBase
    {       
        public UmbracoEventManager()
        {
            File.WriteAllText(@"C:\Temp\Narko.txt", DateTime.Now.ToString());
           
        }
    }

   
}
