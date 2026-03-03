using System;
using System.Collections.Generic;

namespace ZevviCompiler
{
    public readonly struct StateIndex
    {
        private static readonly List<string> names = new();

        public static readonly StateIndex Null = new("Null");
        public static readonly StateIndex Error = new("Error");

        private readonly int index;

        public StateIndex(string name)
        {
            index = names.Count;
            names.Add(name);
        }

        public override bool Equals(object obj)
        {
            return obj is StateIndex s &&
                   index == s.index;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(index);
        }

        public override string ToString()
        {
            return names[index];
        }

        public static implicit operator int(StateIndex s)
        {
            return s.index;
        }

        public static bool operator ==(StateIndex left, StateIndex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StateIndex left, StateIndex right)
        {
            return !(left == right);
        }
    }
}
