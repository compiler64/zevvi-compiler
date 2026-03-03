using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ZevviCompiler.ZI.Operator;

namespace ZevviCompiler.ZI
{
    public class Interpreter
    {
        public DefaultExtension z;
        public Action<string> standardOutput;
        public Func<string> standardInput;

        private Code code;

        public Code Code
        {
            get => code;
            set
            {
                code = value;
                values = new dynamic[code.Length];

                for (int i = 0; i < code.Length; i++)
                {
                    Triple triple = code.triples[i];
                    positions[triple] = i;
                }
            }
        }

        internal readonly Dictionary<Triple, int> positions = new();
        internal dynamic[] values;
        internal readonly Stack<dynamic> allParameters = new();
        private int index;

        internal dynamic[] Parameters => callStack.TryPeek(out ActivationRecord ar) ? ar.Parameters
            : throw new ZIException($"Cannot use ZI parameter when not inside ZI call.");

        internal Dictionary<(int, int), dynamic> Vars { get; } = new();

        internal Dictionary<(int, int), dynamic> LocalVars => callStack.TryPeek(out ActivationRecord ar) ? ar.Locals
            : throw new ZIException($"Cannot use ZI local variable when not inside ZI call.");

        internal Dictionary<(int, int), dynamic> GlobalStorage { get; } = new();

        private readonly Stack<ActivationRecord> callStack = new();
        private bool halt = false;

        public Interpreter(DefaultExtension z, Code code, Action<string> standardOutput = null, Func<string> standardInput = null)
        {
            this.z = z;
            Code = code;
            this.standardOutput = standardOutput ?? Console.Write;
            this.standardInput = standardInput ?? Console.ReadLine;
        }

        private record ActivationRecord(int ReturnAddress, dynamic[] Parameters, Dictionary<(int, int), dynamic> Locals);

        public void Interpret(bool reset = true)
        {
            halt = false;

            if (reset)
            {
                index = 0;
            }

            for (; index < Code.Length && !halt; index++)
            {
                values[index] = InterpretTriple(Code.triples[index]);
            }
        }

        public void Interpret(Code code, bool reset = true)
        {
            if (reset)
            {
                Code = code;
            }
            else
            {
                Code += code;
            }

            Interpret(reset);
        }

        private dynamic Output(string value)
        {
            standardOutput(value);
            return null;
        }

        private string Input()
        {
            return standardInput();
        }

        private dynamic GotoTriple(IOperand storage)
        {
            if (storage is not Triple triple)
            {
                throw new ZIException($"Argument of Goto or similar instruction must be a ZI triple: {storage}.");
            }

            index = positions[triple] - 1;
            return null;
        }

        private dynamic PushParam(dynamic value)
        {
            allParameters.Push(value);
            return null;
        }

        private dynamic CallTriple(IOperand storage, int numParams)
        {
            if (storage is not Triple triple)
            {
                throw new ZIException($"Argument of Goto or similar instruction must be a ZI triple: {storage}.");
            }

            dynamic[] parameters = new dynamic[numParams];

            for (int i = numParams - 1; i >= 0; i--)
            {
                parameters[i] = allParameters.Pop();
            }

            callStack.Push(new ActivationRecord(index, parameters, new()));

            index = positions[triple] - 1;
            return null;
        }

        private dynamic ReturnFromCall(dynamic value)
        {
            index = callStack.Pop().ReturnAddress;
            values[index] = value;
            return null;
        }

        private dynamic HaltInterpreter()
        {
            halt = true;
            return null;
        }

        private dynamic InterpretTriple(Triple triple)
        {
            (Operator op, IOperand arg1, IOperand arg2) = triple;
            dynamic a = arg1?.GetValue(this, 0);
            dynamic b = arg2?.GetValue(this, 0);

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            return op switch
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            {
                DoNothing => null,
                #region Infix Operators
                Add => a + b,
                Subtract => a - b,
                Multiply => a * b,
                IntDivide => a / b,
                Divide => ((double)a) / b,
                Modulo => a % b,
                AddF => a + b,
                SubtractF => a - b,
                MultiplyF => a * b,
                DivideF => a / b,
                ModuloF => a % b,
                And => a & b,
                Or => a | b,
                Xor => a ^ b,
                #endregion
                #region Prefix Operators
                Negate => -a,
                NegateF => -a,
                BitwiseNegate => ~a,
                BooleanNegate => !a,
                ShiftLeft => a << b,
                ShiftRight => a >> b,
                IntToFloat => (double)a,
                FloatToInt => (int)a,
                IntToString => a.ToString(),
                FloatToString => a.ToString(),
                Allocate => throw new NotImplementedException(),
                #endregion
                #region Comparison Operators
                Equal => a == b,
                NotEqual => a != b,
                Greater => a > b,
                Less => a < b,
                GreaterOrEqual => a >= b,
                LessOrEqual => a <= b,
                EqualF => a == b,
                NotEqualF => a != b,
                GreaterF => a > b,
                LessF => a < b,
                GreaterOrEqualF => a >= b,
                LessOrEqualF => a <= b,
                #endregion
                #region I/O
                Operator.Output => Output(a),
                Operator.Input => Input(),
                #endregion
                #region Control Flow
                Goto => GotoTriple(arg1),
                If => a ? GotoTriple(arg2) : null,
                IfNot => a ? null : GotoTriple(arg2),
                IfZero => a == 0 ? GotoTriple(arg2) : null,
                IfNonzero => a != 0 ? GotoTriple(arg2) : null,
                IfPositive => a > 0 ? GotoTriple(arg2) : null,
                IfNegative => a < 0 ? GotoTriple(arg2) : null,
                IfZeroOrPositive => a >= 0 ? GotoTriple(arg2) : null,
                IfZeroOrNegative => a <= 0 ? GotoTriple(arg2) : null,
                IfZeroF => a == 0 ? GotoTriple(arg2) : null,
                IfNonzeroF => a != 0 ? GotoTriple(arg2) : null,
                IfPositiveF => a > 0 ? GotoTriple(arg2) : null,
                IfNegativeF => a < 0 ? GotoTriple(arg2) : null,
                IfZeroOrPositiveF => a >= 0 ? GotoTriple(arg2) : null,
                IfZeroOrNegativeF => a <= 0 ? GotoTriple(arg2) : null,
                Param => PushParam(a),
                Call => CallTriple(arg1, b),
                Return => ReturnFromCall(a),
                Halt => HaltInterpreter(),
                #endregion
                #region Assignment/Pointer Operators
                Copy => arg1.SetValue(this, b, 0),
                LocationOf => (storage: arg1, index: 0),
                ContentsOf => a.storage.GetValue(this, a.index),
                SetContents => a.storage.SetValue(this, b, a.index),
                ItemContents => arg1.GetValue(this, b),
                ItemLocation => (storage: arg1, index: b),
                IndexContents => a.storage.GetValue(this, b + a.index),
                IndexLocation => (a.storage, index: b + a.index),
                #endregion
            };
        }

        public static void Interpret(Code code, DefaultExtension z, Action<string> standardOutput = null, Func<string> standardInput = null)
        {
            new Interpreter(z, code, standardOutput, standardInput).Interpret();
        }
    }
}
