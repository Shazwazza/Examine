using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis;
using Lucene.Net.Facet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Examine
{
    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/>
    /// </summary>
    public static class ServicesCollectionExtensions
    {
        /// <summary>
        /// Registers an Examine index
        /// </summary>
        public static IServiceCollection AddExamineLuceneIndex<TIndex, TDirectoryFactory>(
           this IServiceCollection serviceCollection,
           string name,
           Action<LuceneDirectoryIndexOptions>? configuration = null)
           where TIndex : LuceneIndex
           where TDirectoryFactory : class, IDirectoryFactory
        {
            var config = new LuceneDirectoryIndexOptions();
            configuration?.Invoke(config);

            // This is the long way to add IOptions but gives us access to the
            // services collection which we need to get the dir factory
            serviceCollection.AddSingleton<IConfigureOptions<LuceneDirectoryIndexOptions>>(
                services => new ConfigureNamedOptions<LuceneDirectoryIndexOptions>(
                    name,
                    (options) =>
                    {
                        options.Analyzer = config.Analyzer;
                        options.Validator = config.Validator;
                        options.IndexValueTypesFactory = config.IndexValueTypesFactory;
                        options.FieldDefinitions = config.FieldDefinitions ?? options.FieldDefinitions;
                        options.DirectoryFactory = services.GetRequiredService<TDirectoryFactory>();
                        options.FacetsConfig = config.FacetsConfig ?? new FacetsConfig();
                        options.UseTaxonomyIndex = config.UseTaxonomyIndex;
                    }));

            return serviceCollection.AddSingleton<IIndex>(services =>
            {
                var options = services.GetRequiredService<IOptionsMonitor<LuceneDirectoryIndexOptions>>();

                var index = ActivatorUtilities.CreateInstance<TIndex>(
                    services,
                    new object[] { name, options });

                return index;
            });
        }

        /// <summary>
        /// Registers a file system based Lucene Examine index
        /// </summary>
        public static IServiceCollection AddExamineLuceneIndex(
            this IServiceCollection serviceCollection,
            string name,
            Action<LuceneDirectoryIndexOptions>? configuration = null) => serviceCollection.AddExamineLuceneIndex<LuceneIndex, FileSystemDirectoryFactory>(name, configuration);

        /// <summary>
        /// Registers a file system based Lucene Examine index
        /// </summary>
        public static IServiceCollection AddExamineLuceneIndex<TIndex>(
            this IServiceCollection serviceCollection,
            string name,
            Action<LuceneDirectoryIndexOptions>? configuration = null)
            where TIndex : LuceneIndex => serviceCollection.AddExamineLuceneIndex<TIndex, FileSystemDirectoryFactory>(name, configuration);

        /// <summary>
        /// Registers a Lucene multi index searcher
        /// </summary>
        public static IServiceCollection AddExamineLuceneMultiSearcher(
            this IServiceCollection serviceCollection,
            string name,
            string[] indexNames,
            Action<LuceneMultiSearcherOptions>? configuration = null)
        {
            var config = new LuceneMultiSearcherOptions
            {
                IndexNames = indexNames
            };

            configuration?.Invoke(config);

            // This is the long way to add IOptions but gives us access to the
            // services collection which we need to get the dir factory
            serviceCollection.AddSingleton<IConfigureOptions<LuceneMultiSearcherOptions>>(
                services => new ConfigureNamedOptions<LuceneMultiSearcherOptions>(
                    name,
                    (options) =>
                    {
                        options.Analyzer = config.Analyzer;
                        options.FacetConfiguration = config.FacetConfiguration;
                    }));

            // Transient I think because of how the search context is created, it can't hang on to it.
            return serviceCollection.AddTransient<ISearcher>(s =>
            {
                var namedOptions = s.GetRequiredService<IOptionsMonitor<LuceneMultiSearcherOptions>>().Get(name);
                var matchedIndexes = s.GetServices<IIndex>().Where(x => namedOptions.IndexNames.Contains(x.Name));
                var searcher = ActivatorUtilities.CreateInstance<MultiIndexSearcher>(
                    s,
                    matchedIndexes);

                return searcher;
            });
        }

        /// <summary>
        /// Adds the Examine core services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="appRootDirectory"></param>
        /// <returns></returns>
        public static IServiceCollection AddExamine(this IServiceCollection services, DirectoryInfo? appRootDirectory = null)
        {
            services.TryAddSingleton<IApplicationRoot, CurrentEnvironmentApplicationRoot>();
            services.TryAddSingleton<IExamineManager, ExamineManager>();
            services.TryAddSingleton<IApplicationIdentifier, AspNetCoreApplicationIdentifier>();
            services.TryAddSingleton<ILockFactory, DefaultLockFactory>();

            services.TryAddSingleton<TempEnvFileSystemDirectoryFactory>();

            services.TryAddSingleton<FileSystemDirectoryFactory>(
                s => ActivatorUtilities.CreateInstance<FileSystemDirectoryFactory>(
                    s,
                    new[] { appRootDirectory ?? s.GetRequiredService<IApplicationRoot>().ApplicationRoot }));

            services.TryAddSingleton<SyncedFileSystemDirectoryFactory>(
                s =>
                {
                    var baseDir = appRootDirectory ?? s.GetRequiredService<IApplicationRoot>().ApplicationRoot;

                    var tempDir = TempEnvFileSystemDirectoryFactory.GetTempPath(
                        s.GetRequiredService<IApplicationIdentifier>());

                    return ActivatorUtilities.CreateInstance<SyncedFileSystemDirectoryFactory>(
                        s,
                        new object[]
                        {
                            new DirectoryInfo(tempDir),
                            appRootDirectory ?? s.GetRequiredService<IApplicationRoot>().ApplicationRoot
                        });
                });

            return services;
        }
    }
}
