using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

namespace Examine
{
    public class ExamineDisposeModule : IHttpModule
    {
        public static void Register()
        {
            try
            {
                DynamicModuleUtility.RegisterModule(typeof (ExamineDisposeModule));
            }
            catch
            {
                
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
