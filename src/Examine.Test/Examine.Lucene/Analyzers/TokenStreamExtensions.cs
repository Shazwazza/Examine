using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;

namespace Examine.Test.Examine.Lucene.Analyzers
{
    public static class TokenStreamExtensions
    {
        public static string GetString(this TokenStream @in)
        {
            var @out = new StringBuilder();
            ICharTermAttribute termAtt = @in.AddAttribute<ICharTermAttribute>();
            // extra safety to enforce, that the state is not preserved and also
            // assign bogus values
            @in.ClearAttributes();
            termAtt.SetEmpty().Append("bogusTerm");
            @in.Reset();
            while (@in.IncrementToken())
            {
                if (@out.Length > 0)
                {
                    @out.Append(' ');
                }
                @out.Append(termAtt.ToString());
                @in.ClearAttributes();
                termAtt.SetEmpty().Append("bogusTerm");
            }

            @in.Dispose();
            return @out.ToString();
        }
    }
}
