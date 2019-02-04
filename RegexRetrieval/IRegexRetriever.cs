using System.Collections.Generic;
using RegexRetrieval.Queries;

namespace RegexRetrieval
{
    public interface IRegexRetriever
    {
        IEnumerable<string> Retrieve(Query query, int count);
    }
}
