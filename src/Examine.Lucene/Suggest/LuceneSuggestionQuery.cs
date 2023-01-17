using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine.Search;
using Examine.Suggest;

namespace Examine.Lucene.Suggest
{
    public class LuceneSuggestionQuery : ISuggestionQuery,  ISuggestionExecutor
    {
        private ISuggesterContext _suggesterContext;
        private ISet<string> _sourceFields = null;

        public LuceneSuggestionQuery(ISuggesterContext suggesterContext, SuggestionOptions options)
        {
            _suggesterContext = suggesterContext;
        }

        public ISuggestionQuery SourceFields(ISet<string> fieldNames)
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
            var executor = new LuceneSuggesterExecutor(searchText, options, _sourceFields, _suggesterContext);
            var results = executor.Execute();
            return results;
        }
    }
}
