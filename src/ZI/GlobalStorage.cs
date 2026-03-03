using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler.ZI
{
    [Obsolete("Use ZevviCompiler.ZI.Variable with storage type Global instead.", error: true)]
    public readonly struct GlobalStorage : IOperand
    {
        private static int nextNumber = 0;

        public readonly int number;

        public static GlobalStorage New => new(nextNumber++);

        public GlobalStorage(int number)
        {
            this.number = number;
        }

        public override bool Equals(object obj)
        {
            return obj is GlobalStorage storage &&
                   number == storage.number;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(number);
        }

        public string ToStringSimple()
        {
            return ToStringFull();
        }

        public string ToStringFull()
        {
            return "global" + number;
        }

        public override string ToString()
        {
            return ToStringFull();
        }

        dynamic IOperand.GetValue(Interpreter interpreter, int index)
        {
            return interpreter.GlobalStorage[(number, index)];
        }

        dynamic IOperand.SetValue(Interpreter interpreter, dynamic value, int index)
        {
            interpreter.GlobalStorage[(number, index)] = value;
            return null;
        }

        public static bool operator ==(GlobalStorage left, GlobalStorage right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GlobalStorage left, GlobalStorage right)
        {
            return !(left == right);
        }
    }
}
