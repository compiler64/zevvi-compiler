using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler.ZI
{
    public struct Triple : IOperand
    {
        public static Triple DoNothing => new(Operator.DoNothing);

        public static Triple Halt => new(Operator.Halt);

        private static readonly HashSet<Operator> OpsThatReturnVoid = new()
        {
            Operator.DoNothing,
            Operator.Goto,
            Operator.Halt,
            Operator.IfNegative,
            Operator.IfNegativeF,
            Operator.IfNonzero,
            Operator.IfNonzeroF,
            Operator.IfPositive,
            Operator.IfPositiveF,
            Operator.IfZero,
            Operator.IfZeroF,
            Operator.IfZeroOrNegative,
            Operator.IfZeroOrNegativeF,
            Operator.IfZeroOrPositive,
            Operator.IfZeroOrPositiveF,
            Operator.Return,
            Operator.SetContents,
        };

        private static int nextNumber = 0;

        public readonly int number;
        public readonly Operator op;
        public IOperand arg1;
        public IOperand arg2;

        public Triple(Operator op, IOperand arg1, IOperand arg2)
        {
            number = nextNumber;
            nextNumber++;

            this.op = op;
            this.arg1 = arg1;
            this.arg2 = arg2;

            try
            {
                _ = ToStringFull();
            }
            catch (NullReferenceException)
            {
                throw new ZevviInternalCompilerError("Wrong number of non-null triple arguments.");
            }
        }

        public Triple(Operator op, IOperand arg1) : this(op, arg1, null)
        {
        }

        public Triple(Operator op) : this(op, null, null)
        {
        }

        public string ToStringSimple()
        {
            return "(" + number + ")";
        }

        public string ToStringFull()
        {
            return "(" + number + "):  " + TripleFormatting.Format(op, arg1, arg2);
        }

        public override string ToString()
        {
            return ToStringFull();
        }

        public void Deconstruct(out Operator op, out IOperand arg1, out IOperand arg2)
        {
            op = this.op;
            arg1 = this.arg1;
            arg2 = this.arg2;
        }

        public override bool Equals(object obj)
        {
            return obj is Triple triple &&
                   number == triple.number;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(number);
        }

        dynamic IOperand.GetValue(Interpreter interpreter, int index)
        {
            return interpreter.values[interpreter.positions[this]];
        }

        dynamic IOperand.SetValue(Interpreter interpreter, dynamic value, int index)
        {
            throw new ZIException("Cannot assign to ZI triple in ZI code.");
        }

        public static bool operator ==(Triple left, Triple right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Triple left, Triple right)
        {
            return !(left == right);
        }

        public static Code operator +(Triple left, Triple right)
        {
            return left + (Code)right;
        }

        public static implicit operator Code(Triple triple)
        {
            return new Code(OpsThatReturnVoid.Contains(triple.op) ? null : triple, triple);
        }
    }
}
