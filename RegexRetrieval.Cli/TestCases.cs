namespace RegexRetrieval.Cli
{
    internal class TestCases
    {
        public static readonly string[] DefaultTestCases = new string[]
        {
            // constant words
            "love",
            "the",
            "considerate",
            "qq",
            null,

            // single letters replaced
            "te?t",
            "f?r",
            "?o",
            "g?",
            "?et",
            "te?",
            "c?n?i?e?a?e",
            "c?????????e",
            null,

            // (effectively) only placeholders
            "??",
            "????????",
            "????*",
            "[a]????*",
            "*",
            "[a][b]*[c]",
            null,

            // QMArk and star
            "te?t*",
            "??*abc*",
            "*abc??*",
            "??*abc??*",
            "??*abc??",
            "??abc??*",
            null,

            // stars
            "cons*ate",
            "*ably",
            "*ell*",
            "qq*",
            "*qq",
            "*qq*",
            "*appendchild*",
            null,

            "a*b*c",
            "*a*b*c",
            "a*b*c*",
            "*a*b*c*",
            "*a*b*c*d*e*f*",
            null,

            // char sets
            "initiali[zs]e",
            "[bjp]et",
            null,

            // optionals
            "colo[u]r",
            "[a][b][c]",
            null,

            // option sets (translates to multiple char sets)
            "f{orm}",
            "{abc}",
            "{abcd}",
            "{abcde}",
            "{abcdef}",
            "{abcdefg}",
            "{abcdefgh}",
            null,

            // other
            "*C*", // can be optimized because the English word list does not contain any uppercase letters

            // "*{abcdef}*", // this takes ages and finds >10k words
        };
    }
}
