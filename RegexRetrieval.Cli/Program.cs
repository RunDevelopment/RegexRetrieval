using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using RegexRetrieval.Queries;
using RegexRetrieval.Queries.Parsers;

namespace RegexRetrieval.Cli
{
    internal class Program
    {
        private static int RetrieveCount = 10_000;
        private static int PreviewCount = 10;

        private static string[] __word;
        private static IRegexRetriever __retriever;
        private static IQueryParser __parser = NetspeakQueryParser.Instance;

        private static string[] Words
        {
            get => __word ?? throw new Exception("No words. Please load a text file using `$LOAD pathToFile`");
            set => __word = value;
        }
        private static IRegexRetriever Retriever
        {
            get => __retriever ?? throw new Exception("No retriever. Please create one using `$CREATE`");
            set => __retriever = value;
        }
        private static IQueryParser Parser
        {
            get => __parser ?? throw new Exception("No parser.");
            set => __parser = value;
        }

        private static void Main(string[] args)
        {
            Load("D:\\Text\\words-en.txt");

            var commands = new CommandExecuter();
            commands.AddCommand("LOAD", arguments =>
            {
                if (arguments.Length != 1) throw new ArgumentException("One argument expected.");
                Load(arguments[0]);
            });
            commands.AddCommand("CREATE", arguments =>
            {
                if (arguments.Length > 1) throw new ArgumentException("Zero or one arguments expected.");
                Create(arguments.Length == 1 ? arguments[0] : null);
            });
            commands.AddCommand("TEST", Test);
            commands.AddCommand("GC", _ =>
            {
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                var mem = GC.GetTotalMemory(true);
                Console.WriteLine($"Memory: {mem / 1024.0 / 1024.0:00,0.0}MB");
            });

            string query;
            while ((query = ReadInput()) != "")
            {
                try
                {
                    if (query.StartsWith("$"))
                    {
                        if (!commands.TryExecute(query.Substring(1)))
                            Console.WriteLine("Unknown command");
                    }
                    else
                        ProcessQuery(query);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        private static string ReadInput()
        {
            Console.Write("> ");
            Console.ForegroundColor = ConsoleColor.White;
            var input = Console.ReadLine();
            Console.ResetColor();
            return input;
        }

        private static Query ParseQueryString(string query)
        {
            return new Query(Parser.Parse(query));
        }

        private static void ProcessQuery(string query)
        {
            var watch = Stopwatch.StartNew();

            var results = Retriever.Retrieve(ParseQueryString(query), RetrieveCount).ToList();

            var duration = watch.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
            Console.WriteLine($"{results.Count:##,0} matches in {duration:0.###} ms");

            PreviewResults(results);
        }
        private static void PreviewResults(ICollection<string> words)
        {
            if (PreviewCount <= 0) return;

            int i = 0;
            foreach (var word in words)
            {
                if (i >= PreviewCount) break;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{i++}: ");
                Console.ResetColor();
                Console.WriteLine(word);
            }
            int all = words.Count;
            if (all > i) Console.WriteLine($"and {all - i} other word(s)");
        }

        private static void Load(string path)
        {
            Console.Write("Loading words...");

            var text = File.ReadAllText(path, Encoding.UTF8);
            Words = null;
            Retriever = null;
            GC.Collect();

            // split
            Console.Write("\rSplitting words...");
            Words = Util.GetWords(text);

            Console.WriteLine($"\r{Words.Length:##,#} words, ~{(double) text.Length / Words.Length:0.###} letters/word");
            Console.WriteLine();

            GC.Collect();

            Create();
        }

        private static void Create(string input = null)
        {
            if (input == null)
            {
                Console.WriteLine(@"
Please enter the name of the retriever and its parameters without spaces. E.g.: ""default<1>"".
Available retriever:
    Array
    Default<int maxDepth>
".Trim());
                Console.WriteLine();
                Console.Write("Retriever");

                input = ReadInput();
                Console.WriteLine();
            }

            var methods = new List<(Regex Regex, Func<Match, IRegexRetriever> Factory)>
            {
                // array
                (new Regex(@"(?i)^a(?:rray)?$"),
                m => new ArrayRegexRetriever(Words)),

                // default<maxDepth>
                (new Regex(@"(?i)^d(?:efault)?<(\d+)>$"),
                m => {
                    var maxDepth = int.Parse(m.Groups[1].Value);

                    var options = new RegexRetriever.CreationOptions(true);
                    options.SubStringTrieOptions.MaxDepth = maxDepth;
                    options.LTRPositionSubStringTrieOptions.MaxDepth = maxDepth;
                    options.RTLPositionSubStringTrieOptions.MaxDepth = maxDepth;

                    options.UseWordIndex = true;
                    options.UseLengthMatcher = true;
                    options.UseLTRPositionSubStringTrie = true;
                    options.UseRTLPositionSubStringTrie = true;
                    options.UseSubStringTrie = true;

                    return new RegexRetriever(Words, options);
                }),
            };

            Match lastMatch = null;
            foreach (var (Regex, Factory) in methods)
            {
                lastMatch = Regex.Match(input);
                if (lastMatch.Success)
                {
                    // dispose of old retriever and free memory
                    Retriever = null;
                    GC.Collect();

                    // create new one
                    var watch = Stopwatch.StartNew();
                    Retriever = Factory(lastMatch);
                    Console.WriteLine($"Created in {(double) watch.ElapsedTicks / Stopwatch.Frequency:0.#}s");

                    break;
                }
            }

            if (!(lastMatch?.Success ?? false))
            {
                Console.WriteLine("Invalid input");
            }
            Console.WriteLine($"Current retriever: {__retriever}");
            Console.WriteLine();
        }

        private static void Test(string[] testCases)
        {
            if (testCases.Length == 0) testCases = TestCases.DefaultTestCases;

            var maxLength = testCases.Max(q => q?.Length ?? 0);

            double RunQuery(Query query, out int wordCount)
            {
                var watch = Stopwatch.StartNew();
                wordCount = Retriever.Retrieve(query, 10_000).Count();
                watch.Stop();
                return watch.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
            }
            string ToStrInf(int i) => i == int.MaxValue ? "inf" : i.ToString();

            var table = new MarkdownTable();
            table.AddColumn("Query", maxLength + 2, TextAlignment.Left, q => $"`{q}`");
            table.AddColumn("Avg", 12, TextAlignment.Right, v => $"{v:0.000} ms");
            table.AddColumn("Std", 7, TextAlignment.Right, v => $"{v:0.00}%");
            table.AddColumn("Samples", 7, TextAlignment.Right);
            table.AddColumn("Words", 7, TextAlignment.Right);
            table.AddColumn("Min/Max", 7, TextAlignment.Right);
            table.AddColumn("Combinations", 12, TextAlignment.Right, i => ToStrInf((int) i));

            var infoProvider = Retriever as IInfoProvider;
            if (infoProvider != null)
                foreach (var entry in infoProvider.InfoMetadata)
                    table.AddColumn(entry.Name, entry.Length, TextAlignment.Right);

            table.WriteHeader();

            foreach (var queryString in testCases)
            {
                if (queryString == null)
                {
                    table.WriteEmptyRow();
                    continue;
                }

                var query = ParseQueryString(queryString);

                RunQuery(query, out _); // one for JIT 
                double avg = RunQuery(query, out var wordCount); // one for real
                double std = 0.0;
                var samples = 1;

                // repeat faster measurements as their error is most significant
                if (avg < 500 /* ms */)
                {
                    samples = 1 + (int) Math.Max(Math.Min(499, 250 / avg), 1);
                    var values = Enumerable.Range(0, samples - 1).Select(i => RunQuery(query, out _)).Concat(new[] { avg }).ToList();
                    avg = values.Average();
                    std = Math.Sqrt(values.Sum(v => (v - avg) * (v - avg)) / (samples - 1));
                }

                // write row
                var rowValues = new List<object>()
                {
                    queryString, avg, std / avg * 100, samples, wordCount,
                    $"{ToStrInf(query.MinLength),3}/{ToStrInf(query.MaxLength),3}",
                    query.Combinations
                };
                if (infoProvider != null) rowValues.AddRange(infoProvider.GetInfos());
                table.WriteRow(rowValues);
            }
        }
    }
}
