using System;
using System.Collections.Generic;
using System.Text;

namespace RegexRetrieval.Queries
{
    public interface IQueryTokenizer
    {
        List<QueryToken> Tokenize(string query);
    }
}
