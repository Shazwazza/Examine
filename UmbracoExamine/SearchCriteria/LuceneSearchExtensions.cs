using Examine.SearchCriteria;

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
            return Fuzzy(s, 0.5);
        }

        public static IExamineValue Fuzzy(this string s, double fuzzieness)
        {
            return new ExamineValue(Examineness.Fuzzy, s) { Scope = fuzzieness };
        }

        public static IExamineValue Boost(this string s, double boost)
        {
            return new ExamineValue(Examineness.Boosted, s + "^") { Scope = boost };
        }

        public static IExamineValue Proximity(this string s, double proximity)
        {
            return new ExamineValue(Examineness.Proximity, s) { Scope = proximity };
        }

        public static IExamineValue Excape(this string s)
        {
            return new ExamineValue(Examineness.Escaped, "\"" + s + "\"");
        }

        public static string Then(this IExamineValue vv, string s)
        {
            return vv.Value + s;
        }
    }
}
