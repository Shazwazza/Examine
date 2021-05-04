using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Lucene.Net.Analysis;
using Microsoft.Extensions.DependencyInjection;
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
            DirectoryInfo directory,
            FieldDefinitionCollection fieldDefinitions = null,
            Analyzer analyzer = null,
            IValueSetValidator validator = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null)
            => serviceCollection.AddExamineLuceneIndex<LuceneIndex>(name, directory, fieldDefinitions, analyzer, validator, indexValueTypesFactory);

        /// <summary>
        /// Registers a file system based Lucene Examine index
        /// </summary>
        public static IServiceCollection AddExamineLuceneIndex<TIndex>(
            this IServiceCollection serviceCollection,
            string name,
            DirectoryInfo directory,
            FieldDefinitionCollection fieldDefinitions = null,
            Analyzer analyzer = null,
            IValueSetValidator validator = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null)
            where TIndex : LuceneIndex
            => serviceCollection.AddExamineLuceneIndex<IIndex, FileSystemDirectoryFactory>(name, directory, fieldDefinitions, analyzer, validator, indexValueTypesFactory);

        /// <summary>
        /// Registers an Examine index
        /// </summary>
        public static IServiceCollection AddExamineLuceneIndex<TIndex, TDirectoryFactory>(
            this IServiceCollection serviceCollection,
            string name,
            DirectoryInfo directory,
            FieldDefinitionCollection fieldDefinitions = null,
            Analyzer analyzer = null,
            IValueSetValidator validator = null,
            IReadOnlyDictionary<string, IFieldValueTypeFactory> indexValueTypesFactory = null)
            where TIndex : IIndex
            where TDirectoryFactory : class, IDirectoryFactory
        {
            serviceCollection.AddTransient<TDirectoryFactory>();

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
                        options.FieldDefinitions = fieldDefinitions;

                        var dirFactory = services.GetRequiredService<TDirectoryFactory>();
                        options.IndexDirectory = dirFactory.CreateDirectory(directory);
                    }));

            return serviceCollection.AddTransient<IIndex>(services =>
            {
                LuceneIndex index = ActivatorUtilities.CreateInstance<LuceneIndex>(
                    services,
                    new object[] { name });

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
            Analyzer analyzer = null)
            => serviceCollection.AddExamineSearcher<MultiIndexSearcher>(name, s =>
            {
                IEnumerable<IIndex> matchedIndexes = s.GetServices<IIndex>()
                     .Where(x => indexNames.Contains(x.Name));

                return new List<object>
                    {
                        matchedIndexes,
                        analyzer
                    };
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
