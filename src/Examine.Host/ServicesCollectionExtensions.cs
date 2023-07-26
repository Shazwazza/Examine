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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Examine
{
    /// <summary>
    /// Extensions for <see cref="IServiceCollection"/>
    /// </summary>
    public static class ServicesCollectionExtensions
    {

        /// <summary>
        /// Registers a file system based Lucene Examine index
        /// </summary>
        public static IServiceCollection AddExamineLuceneIndex(
            this IServiceCollection serviceCollection,
            string name,
            FieldDefinitionCollection? fieldDefinitions = null,
            Analyzer? analyzer = null,
            IValueSetValidator? validator = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory>? indexValueTypesFactory = null,
            FacetsConfig? facetsConfig = null)
            => serviceCollection.AddExamineLuceneIndex<LuceneIndex>(name, fieldDefinitions, analyzer, validator, indexValueTypesFactory, facetsConfig);

        /// <summary>
        /// Registers a file system based Lucene Examine index
        /// </summary>
        public static IServiceCollection AddExamineLuceneIndex<TIndex>(
            this IServiceCollection serviceCollection,
            string name,
            FieldDefinitionCollection? fieldDefinitions = null,
            Analyzer? analyzer = null,
            IValueSetValidator? validator = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory>? indexValueTypesFactory = null,
            FacetsConfig? facetsConfig = null)
            where TIndex : LuceneIndex
            => serviceCollection.AddExamineLuceneIndex<TIndex, FileSystemDirectoryFactory>(name, fieldDefinitions, analyzer, validator, indexValueTypesFactory, facetsConfig);

        /// <summary>
        /// Registers an Examine index
        /// </summary>
        public static IServiceCollection AddExamineLuceneIndex<TIndex, TDirectoryFactory>(
            this IServiceCollection serviceCollection,
            string name,
            FieldDefinitionCollection? fieldDefinitions = null,
            Analyzer? analyzer = null,
            IValueSetValidator? validator = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory>? indexValueTypesFactory = null,
            FacetsConfig? facetsConfig = null)
            where TIndex : LuceneIndex
            where TDirectoryFactory : class, IDirectoryFactory
        {
            // This is the long way to add IOptions but gives us access to the
            // services collection which we need to get the dir factory
            serviceCollection.AddSingleton<IConfigureOptions<LuceneDirectoryIndexOptions>>(
                services => new ConfigureNamedOptions<LuceneDirectoryIndexOptions>(
                    name,
                    (options) =>
                    {
                        options.Analyzer = analyzer;
                        options.Validator = validator;
                        options.IndexValueTypesFactory = indexValueTypesFactory;
                        options.FieldDefinitions = fieldDefinitions ?? options.FieldDefinitions;
                        options.DirectoryFactory = services.GetRequiredService<TDirectoryFactory>();
                        options.FacetsConfig = facetsConfig ?? new FacetsConfig();
                    }));

            return serviceCollection.AddSingleton<IIndex>(services =>
            {
                IOptionsMonitor<LuceneDirectoryIndexOptions> options
                        = services.GetRequiredService<IOptionsMonitor<LuceneDirectoryIndexOptions>>();

                TIndex index = ActivatorUtilities.CreateInstance<TIndex>(
                    services,
                    new object[] { name, options });

                return index;
            });
        }

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
        public static IServiceCollection AddExamineLuceneMultiSearcher(
            this IServiceCollection serviceCollection,
            string name,
            string[] indexNames,
            Analyzer analyzer = null,
            FacetsConfig facetsConfig = null)
            => serviceCollection.AddExamineSearcher<MultiIndexSearcher>(name, s =>
            {
                IEnumerable<IIndex> matchedIndexes = s.GetServices<IIndex>()
                     .Where(x => indexNames.Contains(x.Name));

                var parameters = new List<object>
                {
                    matchedIndexes,
                };

                if (facetsConfig != null)
                {
                    parameters.Add(facetsConfig);
                }

                if (analyzer != null)
                {
                    parameters.Add(analyzer);
                }

                return parameters;
            });

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
