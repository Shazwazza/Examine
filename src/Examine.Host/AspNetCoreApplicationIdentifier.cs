using System;
using Examine.Lucene.Directories;
using Microsoft.AspNetCore.DataProtection;

namespace Examine
{
    /// <inheritdoc/>
    public class AspNetCoreApplicationIdentifier : IApplicationIdentifier
    {
        private readonly IServiceProvider _services;

        /// <inheritdoc/>
        public AspNetCoreApplicationIdentifier(IServiceProvider services) => _services = services;


        /// <inheritdoc/>
        public string GetApplicationUniqueIdentifier() => _services.GetApplicationUniqueIdentifier();
    }
}
