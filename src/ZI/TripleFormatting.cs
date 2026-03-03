using System;
using System.Collections.Generic;
using System.Text;
using static ZevviCompiler.ZI.Operator;

namespace ZevviCompiler.ZI
{
    public class TripleFormatting
    {
        private readonly IOperand arg1;
        private readonly IOperand arg2;

        private TripleFormatting(IOperand arg1, IOperand arg2)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
        }

        public static string Format(Operator op, IOperand arg1, IOperand arg2)
        {
            TripleFormatting tripleFormatting = new(arg1, arg2);
            return tripleFormatting.Format(op);
        }

        private string Format(Operator op)
        {
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            return op switch
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
            {
                DoNothing => "do nothing",
                #region Infix Operators
                Add => Infix("+"),
                Subtract => Infix("-"),
                Multiply => Infix("*"),
                IntDivide => Infix("/i"),
                Divide => Infix("/"),
                Modulo => Infix("%"),
                AddF => Infix("+f"),
                SubtractF => Infix("-f"),
                MultiplyF => Infix("*f"),
                DivideF => Infix("/f"),
                ModuloF => Infix("%f"),
                And => Infix("&"),
                Or => Infix("|"),
                Xor => Infix("^"),
                #endregion
                #region Prefix Operators
                Negate => Prefix("-"),
                NegateF => Prefix("-f"),
                BitwiseNegate => Prefix("~"),
                BooleanNegate => Prefix("not"),
                ShiftLeft => Prefix("<<"),
                ShiftRight => Prefix(">>"),
                IntToFloat => Prefix("Int->Float"),
                FloatToInt => Prefix("Float->Int"),
                IntToString => Prefix("Int->String"),
                FloatToString => Prefix("Float->String"),
                Allocate => Prefix("allocate"),
                #endregion
                #region Comparison Operators
                Equal => Infix("=="),
                NotEqual => Infix("!="),
                Greater => Infix(">"),
                Less => Infix("<"),
                GreaterOrEqual => Infix(">="),
                LessOrEqual => Infix("<="),
                EqualF => Infix("==f"),
                NotEqualF => Infix("!=f"),
                GreaterF => Infix(">f"),
                LessF => Infix("<f"),
                GreaterOrEqualF => Infix(">=f"),
                LessOrEqualF => Infix("<=f"),
                #endregion
                #region I/O
                Output => Prefix("output"),
                Input => "input",
                #endregion
                #region Control Flow
                Goto => Prefix("goto"),
                If => IfStmt(""),
                IfNot => IfStmt("", "!"),
                IfZero => IfStmt(" == 0"),
                IfNonzero => IfStmt(" != 0"),
                IfPositive => IfStmt(" > 0"),
                IfNegative => IfStmt(" < 0"),
                IfZeroOrPositive => IfStmt(" >= 0"),
                IfZeroOrNegative => IfStmt(" <= 0"),
                IfZeroF => IfStmt(" == 0f"),
                IfNonzeroF => IfStmt(" != 0f"),
                IfPositiveF => IfStmt(" > 0f"),
                IfNegativeF => IfStmt(" < 0f"),
                IfZeroOrPositiveF => IfStmt(" >= 0f"),
                IfZeroOrNegativeF => IfStmt(" <= 0f"),
                Param => Prefix("param"),
                Call => PrefixBinary("call"),
                Return => PrefixOrCommand("return"),
                Halt => "halt",
                #endregion
                #region Assignment/Pointer Operators
                Copy => Assign(""),
                LocationOf => PrefixNoSpace("&"),
                ContentsOf => PrefixNoSpace("*"),
                SetContents => Assign("*"),
                ItemContents => Index("", ""),
                ItemLocation => Index("&", ""),
                IndexContents => Index("(*", ")"),
                IndexLocation => Index("&(*", ")"),
                #endregion
            };
        }

        private string Infix(string opName)
        {
            return arg1.ToStringSimple() + " " + opName + " " + arg2.ToStringSimple();
        }

        private string Prefix(string opName)
        {
            return opName + " " + arg1.ToStringSimple();
        }

        private string PrefixBinary(string opName)
        {
            return opName + " " + arg1.ToStringSimple() + ", " + arg2.ToStringSimple();
        }

        private string PrefixNoSpace(string opName)
        {
            return opName + arg1.ToStringSimple();
        }

        private string PrefixOrCommand(string opName)
        {
            return opName + (arg1 is null ? "" : " " + arg1.ToStringSimple());
        }

        private string IfStmt(string comparison, string beforeArg1 = "")
        {
            return "if " + beforeArg1 + arg1.ToStringSimple() + comparison + " goto " + arg2.ToStringSimple();
        }

        private string Assign(string beforeArg1)
        {
            return beforeArg1 + arg1.ToStringSimple() + " := " + arg2.ToStringSimple();
        }

        private string Index(string beforeArg1, string afterArg1)
        {
            return beforeArg1 + arg1.ToStringSimple() + afterArg1 + "[" + arg2.ToStringSimple() + "]";
        }
    }
}
