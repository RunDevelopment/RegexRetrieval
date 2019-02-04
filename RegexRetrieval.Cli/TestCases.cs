namespace RegexRetrieval.Cli
{
    internal class TestCases
    {
        public static readonly string[] DefaultTestCases = new string[]
        {
            "love",
            "the",
            "considerate",
            "qq",
            null,

            "te?t",
            "f?r",
            "?o",
            "g?",
            "?et",
            "te?",
            "c?n?i?e?a?e",
            "c?????????e",
            null,

            "??",
            "????????",
            "????*",
            "*",
            "[a][b]*[c]",
            null,

            "te?t*",
            "cons*ate",
            "*ably",
            "*ell*",
            "qq*",
            "*qq",
            "*qq*",
            null,

            "a*b*c",
            "*a*b*c",
            "a*b*c*",
            "*a*b*c*",
            null,

            "initiali[zs]e",
            "[bjp]et",
            "colo[u]r",
            "[a][b][c]",
            null,

            "f{orm}",
            "{abc}",
            "{abcdef}"
        };
    }
}
