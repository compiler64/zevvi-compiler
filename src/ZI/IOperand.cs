using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler.ZI
{
    public interface IOperand
    {
        public string ToStringSimple();

        public string ToStringFull();

        internal dynamic GetValue(Interpreter interpreter, int index);

        internal dynamic SetValue(Interpreter interpreter, dynamic value, int index);

        public static Code AddConst(IOperand storage, int n)
        {
            return storage is Constant<int> ci ? Code.Operand(new Constant<int>(ci.value + n))
                : storage is Variable or Triple ? new Triple(Operator.Add, storage, new Constant<int>(n))
                : throw new ZevviInternalCompilerError("Cannot call AddConst on a non-int ZI constant.");
        }

        public static Code SubtractConst(IOperand storage, int n)
        {
            return storage is Constant<int> ci ? Code.Operand(new Constant<int>(ci.value - n))
                : storage is Variable or Triple ? new Triple(Operator.Subtract, storage, new Constant<int>(n))
                : throw new ZevviInternalCompilerError("Cannot call SubtractConst on a non-int ZI constant.");
        }

        public static Code MultiplyConst(IOperand storage, int n)
        {
            return storage is Constant<int> ci ? Code.Operand(new Constant<int>(ci.value * n))
                : storage is Variable or Triple ? new Triple(Operator.Multiply, storage, new Constant<int>(n))
                : throw new ZevviInternalCompilerError("Cannot call MultiplyConst on a non-int ZI constant.");
        }

        public static Code IntDivideConst(IOperand storage, int n)
        {
            return storage is Constant<int> ci ? Code.Operand(new Constant<int>(ci.value / n))
                : storage is Variable or Triple ? new Triple(Operator.IntDivide, storage, new Constant<int>(n))
                : throw new ZevviInternalCompilerError("Cannot call IntDivideConst on a non-int ZI constant.");
        }
    }
}
