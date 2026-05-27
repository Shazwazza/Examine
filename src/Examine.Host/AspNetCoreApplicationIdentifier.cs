using System;
using Examine.Lucene.Directories;
using Microsoft.AspNetCore.DataProtection;

namespace Examine
{

    public class AspNetCoreApplicationIdentifier : IApplicationIdentifier
    {
        private readonly IServiceProvider _services;
        public AspNetCoreApplicationIdentifier(IServiceProvider services) => _services = services;
        public string GetApplicationUniqueIdentifier() => _services.GetApplicationUniqueIdentifier();
    }
}
