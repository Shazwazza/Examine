using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Search;

namespace Examine.Lucene.Scoring
{
    public interface IScoringProfile
    {
        Query GetScoreQuery(Query inner);
    }
}
