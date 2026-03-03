using System;
using System.Collections.Generic;

namespace ZevviCompiler
{
    public readonly struct Nonterminal
    {
        private static readonly List<string> names = new();

        public static readonly Nonterminal nNull = new("Null");
        public static readonly Nonterminal nEofToken = new("EofToken");
        public static readonly Nonterminal nCharToken = new("CharToken");
        public static readonly Nonterminal nStrToken = new("StringToken");
        public static readonly Nonterminal nIntToken = new("IntToken");
        public static readonly Nonterminal nFloatToken = new("FloatToken");
        public static readonly Nonterminal nIdToken = new("IdToken");

        public static Nonterminal ForToken(TokenType tokenType)
        {
            return tokenType switch
            {
                TokenType.Eof => nEofToken,
                TokenType.Char => nCharToken,
                TokenType.Str => nStrToken,
                TokenType.Int => nIntToken,
                TokenType.Float => nFloatToken,
                TokenType.Id => nIdToken,
                _ => throw new UnknownEnumValueInternalException(nameof(TokenType)),
            };
        }

        private readonly int index;

        public Nonterminal(string name)
        {
            index = names.Count;
            names.Add(name);
        }

        public override bool Equals(object obj)
        {
            return obj is Nonterminal n &&
                   index == n.index;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(index);
        }

        public override string ToString()
        {
            return names[index];
        }

        public static implicit operator int(Nonterminal s)
        {
            return s.index;
        }

        public static bool operator ==(Nonterminal left, Nonterminal right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Nonterminal left, Nonterminal right)
        {
            return !(left == right);
        }
    }
}
