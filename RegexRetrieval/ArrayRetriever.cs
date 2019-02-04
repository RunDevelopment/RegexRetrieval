using System.Collections.Generic;
using System.Linq;
using RegexRetrieval.Queries;

namespace RegexRetrieval
{
    public class ArrayRegexRetriever : IRegexRetriever
    {
        public string[] Words { get; }

        public ArrayRegexRetriever(string[] words)
        {
            Words = words;
        }

        public IEnumerable<string> Retrieve(Query query, int count)
        {
            return Words.Where(query.CreatePredicate()).Take(count);
        }
    }
}
