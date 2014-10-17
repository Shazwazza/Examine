using System.Web;
using System.Web.Hosting;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

namespace Examine.Session
{
    /// <summary>
    /// This dynamically registers itself on pre-init and is used to ensure that everything is disposed of properly at the end of http requests
    /// </summary>
    public sealed class ExamineDisposeModule : IHttpModule
    {
        public static void Register()
        {
            try
            {
                if (HostingEnvironment.IsHosted)
                {
                    DynamicModuleUtility.RegisterModule(typeof(ExamineDisposeModule));
                }                
            }
            catch
            {
                //probably not in a website!
            }
        }

        public void Init(HttpApplication context)
        {
            context.EndRequest += (o, eventArgs) =>
            {                
                if (ExamineManager.InstanceInitialized)
                {
                    ExamineManager.Instance.EndRequest();
                }
            };
        }

        public void Dispose()
        {

        }
    }
}
