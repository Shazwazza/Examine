using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Examine
{

    public static class ServicesCollectionExtensions
    {

        /// <summary>
        /// Registers a file system based Lucene Examine index
        /// </summary>
        /// <typeparam name="TIndex"></typeparam>
        /// <typeparam name="TDirectoryFactory"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="name"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static IServiceCollection AddExamineLuceneIndex(
            this IServiceCollection serviceCollection,
            string name,
            DirectoryInfo directory)
            => serviceCollection.AddTransient<IIndex>(services =>
            {
                IDirectoryFactory dirFactory = services.GetRequiredService<FileSystemDirectoryFactory>();
                global::Lucene.Net.Store.Directory dir = dirFactory.CreateDirectory(directory);

                LuceneIndex index = ActivatorUtilities.CreateInstance<LuceneIndex>(
                    services,
                    name,
                    dir);

                return index;
            });

        /// <summary>
        /// Registers a file system based Lucene Examine index
        /// </summary>
        /// <typeparam name="TIndex"></typeparam>
        /// <typeparam name="TDirectoryFactory"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="name"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static IServiceCollection AddExamineLuceneIndex<TIndex>(
            this IServiceCollection serviceCollection,
            string name,
            DirectoryInfo directory)
            where TIndex : LuceneIndex
            => serviceCollection.AddTransient<IIndex>(services =>
            {
                IDirectoryFactory dirFactory = services.GetRequiredService<FileSystemDirectoryFactory>();
                global::Lucene.Net.Store.Directory dir = dirFactory.CreateDirectory(directory);

                TIndex index = ActivatorUtilities.CreateInstance<TIndex>(
                    services,
                    name,
                    dir);

                return index;
            });

        /// <summary>
        /// Registers an Examine index
        /// </summary>
        /// <typeparam name="TIndex"></typeparam>
        /// <typeparam name="TDirectoryFactory"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="name"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static IServiceCollection AddExamineLuceneIndex<TIndex, TDirectoryFactory>(
            this IServiceCollection serviceCollection,
            string name,
            DirectoryInfo directory)
            where TIndex : IIndex
            where TDirectoryFactory : IDirectoryFactory
            => serviceCollection.AddTransient<IIndex>(services =>
                {
                    TDirectoryFactory dirFactory = services.GetRequiredService<TDirectoryFactory>();
                    global::Lucene.Net.Store.Directory dir = dirFactory.CreateDirectory(directory);

                    TIndex index = ActivatorUtilities.CreateInstance<TIndex>(
                        services,
                        name,
                        dir);

                    return index;
                });

        /// <summary>
        /// Registers an Examine index
        /// </summary>
        /// <typeparam name="TIndex"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="name"></param>
        /// <param name="parameterFactory">
        /// A factory to fullfill the custom index construction parameters excluding the name that are not already registerd in DI.
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddExamineIndex<TIndex>(
            this IServiceCollection serviceCollection,
            string name,
            Func<IServiceProvider, IList<object>> parameterFactory)
            where TIndex : IIndex
            => serviceCollection.AddTransient<IIndex>(services =>
            {
                IList<object> parameters = parameterFactory(services);
                parameters.Insert(0, name);

                TIndex index = ActivatorUtilities.CreateInstance<TIndex>(
                    services,
                    parameters.ToArray());

                return index;
            });

        /// <summary>
        /// Registers a standalone Examine searcher
        /// </summary>
        /// <typeparam name="TSearcher"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="name"></param>
        /// <param name="parameterFactory">
        /// A factory to fullfill the custom searcher construction parameters excluding the name that are not already registerd in DI.
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddExamineSearcher<TSearcher>(
            this IServiceCollection serviceCollection,
            string name,
            Func<IServiceProvider, IList<object>> parameterFactory)
            where TSearcher : ISearcher
           => serviceCollection.AddTransient<ISearcher>(services =>
           {
               IList<object> parameters = parameterFactory(services);
               parameters.Insert(0, name);

               TSearcher searcher = ActivatorUtilities.CreateInstance<TSearcher>(
                   services,
                   parameters.ToArray());

               return searcher;
           });

        /// <summary>
        /// Registers a lucene multi index searcher
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="name"></param>
        /// <param name="indexNames"></param>        
        /// <returns></returns>
        public static IServiceCollection AddExamineLuceneMultiSearcher(
            this IServiceCollection serviceCollection,
            string name,
            string[] indexNames)
           => serviceCollection.AddTransient<ISearcher>(services =>
           {
               IEnumerable<IIndex> matchedIndexes = services.GetServices<IIndex>()
                    .Where(x => indexNames.Contains(x.Name));

               var parameters = new List<object>
               {
                   name,
                   matchedIndexes
               };

               var searcher = ActivatorUtilities.CreateInstance<MultiIndexSearcher>(
                   services,
                   parameters.ToArray());

               return searcher;
           });

        /// <summary>
        /// Adds the Examine core services
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddExamine(this IServiceCollection services)
        {
            services.AddSingleton<IExamineManager, ExamineManager>();
            services.AddSingleton<IApplicationIdentifier, AspNetCoreApplicationIdentifier>();
            services.AddSingleton<ILockFactory, DefaultLockFactory>();
            services.AddSingleton<SyncMutexManager>();
            services.AddSingleton<SyncTempEnvDirectoryFactory>();
            services.AddSingleton<TempEnvDirectoryFactory>();
            services.AddSingleton<FileSystemDirectoryFactory>();

            return services;
        }
    }
}
