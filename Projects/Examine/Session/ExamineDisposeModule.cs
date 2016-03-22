using System.Web;
using System.Web.Hosting;
using Examine.Session;

[assembly: PreApplicationStartMethod(typeof(ExamineDisposeModule), "Register")]

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
                    HttpApplication.RegisterModule(typeof(ExamineDisposeModule));
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
