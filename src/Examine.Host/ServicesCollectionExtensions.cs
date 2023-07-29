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
        /// Registers a file system based Lucene Examine index
        /// </summary>
        [Obsolete("To remove in Examine V5")]
        public static IServiceCollection AddExamineLuceneIndex(
            this IServiceCollection serviceCollection,
            string name,
            FieldDefinitionCollection fieldDefinitions = null,
            Analyzer analyzer = null,
            IValueSetValidator validator = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null)
            => serviceCollection.AddExamineLuceneIndex<LuceneIndex>(name, fieldDefinitions, analyzer, validator, indexValueTypesFactory);

        /// <summary>
        /// Registers a file system based Lucene Examine index
        /// </summary>
        [Obsolete("To remove in Examine V5")]
        public static IServiceCollection AddExamineLuceneIndex<TIndex>(
            this IServiceCollection serviceCollection,
            string name,
            FieldDefinitionCollection fieldDefinitions = null,
            Analyzer analyzer = null,
            IValueSetValidator validator = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null)
            where TIndex : LuceneIndex
        {
            Action<ExamineLuceneIndexConfiguration<TIndex, FileSystemDirectoryFactory>> config = opt =>
            {
                opt.FieldDefinitions = fieldDefinitions;
                opt.Analyzer = analyzer;
                opt.UseTaxonomyIndex = false;
                opt.FacetsConfig = null;
                opt.IndexValueTypesFactory = indexValueTypesFactory;
                opt.Validator = validator;
            };
            return serviceCollection.AddExamineLuceneIndex<TIndex, FileSystemDirectoryFactory>(name, config);
        }

        /// <summary>
        /// Registers an Examine index
        /// </summary>
        [Obsolete("To remove in Examine V5")]
        public static IServiceCollection AddExamineLuceneIndex<TIndex, TDirectoryFactory>(
            this IServiceCollection serviceCollection,
            string name,
            FieldDefinitionCollection fieldDefinitions = null,
            Analyzer analyzer = null,
            IValueSetValidator validator = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null)
            where TIndex : LuceneIndex
            where TDirectoryFactory : class, IDirectoryFactory
        {
            Action<ExamineLuceneIndexConfiguration<TIndex, TDirectoryFactory>> config = opt =>
            {
                opt.FieldDefinitions = fieldDefinitions;
                opt.Analyzer = analyzer;
                opt.UseTaxonomyIndex = false;
                opt.FacetsConfig = null;
                opt.IndexValueTypesFactory = indexValueTypesFactory;
                opt.Validator = validator;
            };
            return serviceCollection.AddExamineLuceneIndex<TIndex, TDirectoryFactory>(name, config);
        }

        /// <summary>
        /// Registers an Examine index
        /// </summary>
        public static IServiceCollection AddExamineLuceneIndex<TIndex, TDirectoryFactory>(
           this IServiceCollection serviceCollection,
           string name,
           Action<ExamineLuceneIndexConfiguration<TIndex, TDirectoryFactory>> configuration)
           where TIndex : LuceneIndex
           where TDirectoryFactory : class, IDirectoryFactory
        {
            var config = new ExamineLuceneIndexConfiguration<TIndex, TDirectoryFactory>(name);
            configuration.Invoke(config);

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
                IOptionsMonitor<LuceneDirectoryIndexOptions> options
                        = services.GetRequiredService<IOptionsMonitor<LuceneDirectoryIndexOptions>>();

                TIndex index = ActivatorUtilities.CreateInstance<TIndex>(
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
            Action<ExamineLuceneIndexConfiguration<LuceneIndex, FileSystemDirectoryFactory>> configuration)
        {
            return serviceCollection.AddExamineLuceneIndex<LuceneIndex, FileSystemDirectoryFactory>(name, configuration);
        }

        /// <summary>
        /// Registers a file system based Lucene Examine index
        /// </summary>
        public static IServiceCollection AddExamineLuceneIndex<TIndex>(
            this IServiceCollection serviceCollection,
            string name,
            Action<ExamineLuceneIndexConfiguration<TIndex, FileSystemDirectoryFactory>> configuration)
            where TIndex : LuceneIndex
        {
            return serviceCollection.AddExamineLuceneIndex<TIndex, FileSystemDirectoryFactory>(name, configuration);
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
        [Obsolete("Will be removed in Examine V5")]
        public static IServiceCollection AddExamineLuceneMultiSearcher(
            this IServiceCollection serviceCollection,
            string name,
            string[] indexNames,
            Analyzer analyzer = null)
        {
            var cfg = new Action<ExamineLuceneMultiSearcherConfiguration>(opt =>
            {
                opt.Analyzer = analyzer;
                opt.FacetConfiguration = default;
            });
            return AddExamineLuceneMultiSearcher(serviceCollection, name, indexNames, cfg);
        }


        /// <summary>
        /// Registers a lucene multi index searcher
        /// </summary>
        public static IServiceCollection AddExamineLuceneMultiSearcher(
            this IServiceCollection serviceCollection,
            string name,
            string[] indexNames,
            Action<ExamineLuceneMultiSearcherConfiguration> configuration = null)
        {
            var cfg = new ExamineLuceneMultiSearcherConfiguration(name, indexNames);
            configuration?.Invoke(cfg);
            return serviceCollection.AddExamineSearcher<MultiIndexSearcher>(name, s =>
                    {
                        IEnumerable<IIndex> matchedIndexes = s.GetServices<IIndex>()
                             .Where(x => indexNames.Contains(x.Name));

                        var parameters = new List<object>
                        {
                    matchedIndexes,
                        };

                        if (cfg.FacetConfiguration != null)
                        {
                            parameters.Add(cfg.FacetConfiguration);
                        }

                        if (cfg.Analyzer != null)
                        {
                            parameters.Add(cfg.Analyzer);
                        }

                        return parameters;
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
