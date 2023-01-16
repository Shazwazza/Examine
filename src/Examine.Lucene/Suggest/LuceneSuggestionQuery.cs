using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Search;
using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    public class LuceneSuggestionQuery : ISuggestionQuery, ISuggestionOrdering, ISuggestionSelectFields, ISuggestionExecutor
    {
        private ISuggesterContext _suggesterContext;
        private SuggestionOptions _options;
        private ISet<string> _fieldsToLoad = null;
        private ISet<string> _sourceFields = null;

        public LuceneSuggestionQuery(ISuggesterContext suggesterContext, SuggestionOptions options)
        {
            _suggesterContext = suggesterContext;
            _options = options;
        }

        public ISuggestionSelectFields OrderBy(params SortableField[] fields) => throw new NotImplementedException();
        public ISuggestionSelectFields OrderByDescending(params SortableField[] fields) => throw new NotImplementedException();
        public ISuggestionSelectFields SelectFields(ISet<string> fieldNames)
        {
            _fieldsToLoad = fieldNames;
            return this;
        }
        public ISuggestionOrdering SourceFields(ISet<string> fieldNames)
        {
            if (fieldNames == null)
            {
                throw new ArgumentNullException(nameof(fieldNames));
            }
            if(fieldNames.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fieldNames));
            }
            _sourceFields = fieldNames;
            return this;
        }


        public ISuggestionResults Execute(string searchText, SuggestionOptions options = null)
        {
            var executor = new LuceneSuggesterExecutor(searchText, options, _sourceFields, _fieldsToLoad, _suggesterContext);
            var results = executor.Execute();
            return results;
        }
    }
}
