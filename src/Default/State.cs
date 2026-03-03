using System;

namespace ZevviCompiler
{
    public readonly struct State
    {
        public readonly StateIndex index;
        public readonly int numToPop;
        public readonly bool canMerge;
        public readonly MergeFunc Merge;

        public static readonly State Null = new(StateIndex.Null, 0);
        public static readonly State Error = new(StateIndex.Error, 0);

        public State(StateIndex index, int numToPop, MergeFunc Merge = null)
        {
            this.index = index;
            this.numToPop = numToPop;
            this.canMerge = Merge != null;
            this.Merge = Merge;
        }

        public override bool Equals(object other)
        {
            return other is State s && index == s.index;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(index);
        }

        public override string ToString()
        {
            return $"{index}({numToPop})";
        }

        public static bool operator ==(State left, State right) => left.Equals(right);

        public static bool operator !=(State left, State right) => !left.Equals(right);
    }
}
