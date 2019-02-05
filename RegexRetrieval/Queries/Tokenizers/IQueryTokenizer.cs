using System.Collections.Generic;

namespace RegexRetrieval.Queries.Tokenizers
{
    public interface IQueryTokenizer
    {
        List<QueryToken> Tokenize(string query);
    }
}
