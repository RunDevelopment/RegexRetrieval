using System;

namespace RegexRetrieval.Queries
{
    public readonly struct QueryToken
    {
        public readonly Type TokenType;
        public readonly string Value;

        private QueryToken(Type tokenType, string value)
        {
            TokenType = tokenType;
            Value = value;
        }

        public static QueryToken CreateWord(string value)
            => new QueryToken(Type.Words, value);
        public static QueryToken CreateQMark()
            => new QueryToken(Type.QMark, null);
        public static QueryToken CreateStar()
            => new QueryToken(Type.Star, null);
        public static QueryToken CreateCharSet(string value)
            => new QueryToken(Type.CharSet, value);
        public static QueryToken CreateOptional(string value)
            => new QueryToken(Type.Optional, value);

        public override string ToString()
        {
            switch (TokenType)
            {
                case Type.Words:
                    return Value;
                case Type.QMark:
                    return "?";
                case Type.Star:
                    return "*";
                case Type.CharSet:
                    return "[" + Value + "]";
                case Type.Optional:
                    return "{" + Value + "}";
                default:
                    throw new InvalidOperationException();
            }
        }

        public enum Type
        {
            Words,
            QMark,
            Star,
            CharSet,
            Optional
        }
    }
}
