using System.Collections.Generic;

namespace RegexRetrieval.Queries.Parsers
{
    public interface IQueryParser
    {
        List<QueryToken> Parse(string query);
    }
}
