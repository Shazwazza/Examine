using Examine.Lucene.Directories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Examine
{

    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection AddExamine(this IServiceCollection services)
        {
            services.AddSingleton<IExamineManager, ExamineManager>();
            services.AddSingleton<IApplicationIdentifier, AspNetCoreApplicationIdentifier>();
            services.AddSingleton<SyncMutexManager>();
            services.AddSingleton<SyncTempEnvDirectoryFactory>();
            services.AddSingleton<TempEnvDirectoryFactory>();
            services.AddSingleton<FileSystemDirectoryFactory>();

            return services;
        }
    }
}
