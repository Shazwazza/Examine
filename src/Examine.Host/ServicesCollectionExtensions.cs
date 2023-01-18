using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Examine.Lucene.Suggest;
using Examine.Suggest;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Examine
{
    public static class ServicesCollectionExtensions
    {
        /// <summary>
        /// Registers a file system based Lucene Examine index
        /// </summary>
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
        public static IServiceCollection AddExamineLuceneIndex<TIndex>(
            this IServiceCollection serviceCollection,
            string name,
            FieldDefinitionCollection fieldDefinitions = null,
            Analyzer analyzer = null,
            IValueSetValidator validator = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null)
            where TIndex : LuceneIndex
            => serviceCollection.AddExamineLuceneIndex<TIndex, FileSystemDirectoryFactory>(name, fieldDefinitions, analyzer, validator, indexValueTypesFactory);

        /// <summary>
        /// Registers an Examine index
        /// </summary>
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
        /// A factory to fullfill the custom searcher construction parameters excluding the name that are not already registered in DI.
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
            Analyzer analyzer = null)
            => serviceCollection.AddExamineSearcher<MultiIndexSearcher>(name, s =>
            {
                IEnumerable<IIndex> matchedIndexes = s.GetServices<IIndex>()
                     .Where(x => indexNames.Contains(x.Name));

                var parameters = new List<object>
                {
                    matchedIndexes
                };

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
        /// <returns></returns>
        public static IServiceCollection AddExamine(this IServiceCollection services, DirectoryInfo appRootDirectory = null)
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

        /// <summary>
        /// Registers a standalone Examine suggester
        /// </summary>
        /// <typeparam name="TSearcher"></typeparam>
        /// <param name="serviceCollection"></param>
        /// <param name="name"></param>
        /// <param name="parameterFactory">
        /// A factory to fullfill the custom suggester construction parameters excluding the name that are not already registerd in DI.
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddExamineSuggester<TSuggester>(
            this IServiceCollection serviceCollection,
            string name,
            Func<IServiceProvider, IList<object>> parameterFactory)
            where TSuggester : ISuggester
           => serviceCollection.AddTransient<ISuggester>(services =>
           {
               IList<object> parameters = parameterFactory(services);
               parameters.Insert(0, name);

               ISuggester suggester = ActivatorUtilities.CreateInstance<TSuggester>(
                   services,
                   parameters.ToArray());

               return suggester;
           });

        /// <summary>
        /// Registers a lucene suggester
        /// </summary>
        public static IServiceCollection AddExamineLuceneSuggester(
            this IServiceCollection serviceCollection,
            string name,
            string indexName,
            Analyzer queryAanalyzer = null)
            => serviceCollection.AddExamineSuggester<LuceneSuggester>(name, s =>
            {
                IIndex matchedIndex = s.GetServices<IIndex>()
                     .First(x => x.Name.Equals(indexName));
                if (!(matchedIndex is LuceneIndex luceneIndex))
                {
                    throw new InvalidOperationException("LuceneSuggester can not operate on non Lucene Indexes");
                }

                var parameters = new List<object>
                {
                    luceneIndex,
                    luceneIndex.FieldValueTypeCollection,
                    queryAanalyzer
                };

                return parameters;
            });
    }
}
