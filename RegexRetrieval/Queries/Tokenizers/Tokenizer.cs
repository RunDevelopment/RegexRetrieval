using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RegexRetrieval.Queries.Tokenizers
{
    public class Tokenizer<StateT>
    {
        private readonly List<(Regex Regex, CaseAction Action)> cases = new List<(Regex, CaseAction)>();

        /// <summary>
        /// The default action if no case matches.
        /// <para>
        /// This will be given the state, the string to parse and the current position.
        /// It will return the number of characters to advance (this has to be greater than 0).
        /// </para>
        /// </summary>
        public DefaultAction Default { get; set; } = (state, s, i) => throw new InvalidOperationException();

        public void AddCase(Regex regex, SimpleCaseAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            AddCase(regex, (s, str, m) =>
            {
                action(s, str, m);
                return m.Length;
            });
        }
        public void AddCase(Regex regex, CaseAction action)
        {
            if (regex == null) throw new ArgumentNullException(nameof(regex));
            if (action == null) throw new ArgumentNullException(nameof(action));

            cases.Add((regex, action));
        }

        public void Tokenize(string str, StateT state)
        {
            var p = 0;
            var caseCount = cases.Count;

            while (p < str.Length)
            {
                Match m = null;

                for (var i = 0; i < caseCount; i++)
                {
                    var _case = cases[i];
                    m = _case.Regex.Match(str, p);

                    if (m.Success && m.Index == p)
                    {
                        _case.Action(state, str, m);
                        p += m.Length;
                        break;
                    }
                }

                if (m == null || !m.Success)
                    p += Default(state, str, p);
            }
        }

        public static readonly SimpleCaseAction Skip = (state, s, m) => { };

        public delegate void SimpleCaseAction(StateT state, string str, Match match);
        public delegate int CaseAction(StateT state, string str, Match match);
        public delegate int DefaultAction(StateT state, string str, int index);
    }
}
