using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler.ZI
{
    public enum Operator
    {
        DoNothing,
        #region Infix Operators
        Add,
        Subtract,
        Multiply,
        IntDivide,
        Divide,
        Modulo,
        AddF,
        SubtractF,
        MultiplyF,
        DivideF,
        ModuloF,
        And,
        Or,
        Xor,
        #endregion
        #region Prefix Operators
        Negate,
        NegateF,
        BitwiseNegate,
        BooleanNegate,
        ShiftLeft,
        ShiftRight,
        IntToFloat,
        FloatToInt,
        IntToString,
        FloatToString,
        Allocate,
        #endregion
        #region Comparison Operators
        Equal,
        NotEqual,
        Greater,
        Less,
        GreaterOrEqual,
        LessOrEqual,
        EqualF,
        NotEqualF,
        GreaterF,
        LessF,
        GreaterOrEqualF,
        LessOrEqualF,
        #endregion
        #region I/O
        Output,
        Input,
        #endregion
        #region Control Flow
        Goto,
        If,
        IfNot,
        IfZero,
        IfNonzero,
        IfPositive,
        IfNegative,
        IfZeroOrPositive,
        IfZeroOrNegative,
        IfZeroF,
        IfNonzeroF,
        IfPositiveF,
        IfNegativeF,
        IfZeroOrPositiveF,
        IfZeroOrNegativeF,
        Param,
        Call,
        Return,
        Halt,
        #endregion
        #region Assignment/Pointer Operators
        Copy,
        LocationOf,
        ContentsOf,
        SetContents,
        ItemContents,
        ItemLocation,
        IndexContents,
        IndexLocation,
        #endregion
    }
}
