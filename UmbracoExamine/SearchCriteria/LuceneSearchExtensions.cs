using Examine.SearchCriteria;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;

namespace UmbracoExamine.SearchCriteria
{
    public static class LuceneSearchExtensions
    {
        public static IExamineValue SingleCharacterWildcard(this string s)
        {
            return new ExamineValue(Examineness.SimpleWildcard, s + "?");
        }

        public static IExamineValue MultipleCharacterWildcard(this string s)
        {
            return new ExamineValue(Examineness.ComplexWildcard, s + "*");
        }

        public static IExamineValue Fuzzy(this string s)
        {
            return Fuzzy(s, 0.5f);
        }

        public static IExamineValue Fuzzy(this string s, float fuzzieness)
        {
            return new ExamineValue(Examineness.Fuzzy, s, fuzzieness);
        }

        public static IExamineValue Boost(this string s, float boost)
        {
            return new ExamineValue(Examineness.Boosted, s + "^", boost);
        }

        public static IExamineValue Proximity(this string s, float proximity)
        {
            return new ExamineValue(Examineness.Proximity, s, proximity);
        }

        public static IExamineValue Escape(this string s)
        {
            return new ExamineValue(Examineness.Escaped, QueryParser.Escape(s));
        }

        public static string Then(this IExamineValue vv, string s)
        {
            return vv.Value + s;
        }

        public static BooleanClause.Occur ToLuceneOccurance(this BooleanOperation o)
        {
            switch (o)
            {
                case BooleanOperation.And:
                    return BooleanClause.Occur.MUST;
                case BooleanOperation.Not:
                    return BooleanClause.Occur.MUST_NOT;
                case BooleanOperation.Or:
                default:
                    return BooleanClause.Occur.SHOULD;
            }
        }

        public static BooleanOperation ToBooleanOperation(this BooleanClause.Occur o)
        {
            if (o == BooleanClause.Occur.MUST)
            {
                return BooleanOperation.And;
            }
            else if (o == BooleanClause.Occur.MUST_NOT)
            {
                return BooleanOperation.Not;
            }
            else
            {
                return BooleanOperation.Or;
            }
        }
    }
}
