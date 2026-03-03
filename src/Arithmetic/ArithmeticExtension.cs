using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZevviCompiler.OperatorSyntaxes;

namespace ZevviCompiler.Arithmetic
{
    /// <summary>
    /// A Zevvi compiler extension with the arithmetic, bitwise, comparison, and boolean operators.
    /// Contains the following operators:<br/>
    /// <c>+ - * / % ** &amp; | ^ ~ == != &lt; &gt; &lt;= &gt;= &amp;&amp; || ^^ !</c><br/>
    /// Requirements: <see cref="OperatorSyntaxesExtension"/>.
    /// </summary>
    public class ArithmeticExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(OperatorSyntaxesExtension) };
        
        private OperatorSyntaxesExtension OpExt => Z.GetExtension<OperatorSyntaxesExtension>();

        #region Operator Precedences
        public const int P_BOOLEAN_OR_L = 6000;
        public const int P_BOOLEAN_OR_R = 6001;
        public const int P_BOOLEAN_XOR_L = 7000;
        public const int P_BOOLEAN_XOR_R = 7001;
        public const int P_BOOLEAN_AND_L = 8000;
        public const int P_BOOLEAN_AND_R = 8001;
        public const int P_COMPARISON_L = 9000;
        public const int P_COMPARISON_R = 9001;
        public const int P_BITWISE_OR_L = 10000;
        public const int P_BITWISE_OR_R = 10001;
        public const int P_BITWISE_XOR_L = 11000;
        public const int P_BITWISE_XOR_R = 11001;
        public const int P_BITWISE_AND_L = 12000;
        public const int P_BITWISE_AND_R = 12001;
        public const int P_ADDITIVE_L = 13000;
        public const int P_ADDITIVE_R = 13001;
        public const int P_MULTIPLICATIVE_L = 14000;
        public const int P_MULTIPLICATIVE_R = 14001;
        public const int P_EXPONENTIATIVE_L = 15000;
        public const int P_EXPONENTIATIVE_R = 15001;
        public const int P_BOOLEAN_NOT_R = 16000;
        public const int P_BITWISE_NOT_R = 17000;
        #endregion

        // TODO add floats and exponents
        public OperatorOverloadDict canAdd, canSubtract, canMultiply, canIntDivide, canDivide, canModulo;
        public OperatorOverloadDict canBitwiseAnd, canBitwiseOr, canBitwiseXor;
        public OperatorOverloadDict canBooleanAnd, canBooleanOr, canBooleanXor;
        public OperatorOverloadDict canEquate, canInequate, canBeLess, canBeGreater, canBeLessOrEqual, canBeGreaterOrEqual;
        public OperatorOverloadDict canUnaryPlus, canNegate, canNegateBitwise, canNegateBoolean;

        public override void InitConverts()
        {
            Z.Int.Converter.AutoConvert.Add(Z.Float, storage => new ZI.Triple(ZI.Operator.IntToFloat, storage));
            Z.Float.Converter.ExplicitConvert.Add(Z.Int, storage => new ZI.Triple(ZI.Operator.FloatToInt, storage));
        }

        public override void InitStates()
        {
        }

        public override void InitSymbolTable()
        {
            Z.symbolTable.Add("true", Z.Bool, new ZI.Constant<bool>(true));
            Z.symbolTable.Add("false", Z.Bool, new ZI.Constant<bool>(false));

            OpExt.NewPrefixOrInfix("+", P_ADDITIVE_L, P_ADDITIVE_R, canUnaryPlus, canAdd);
            OpExt.NewPrefixOrInfix("-", P_ADDITIVE_L, P_ADDITIVE_R, canNegate, canSubtract);

            OpExt.NewInfix("*", P_MULTIPLICATIVE_L, P_MULTIPLICATIVE_R, canMultiply);
            OpExt.NewInfix("\\", P_MULTIPLICATIVE_L, P_MULTIPLICATIVE_R, canIntDivide);
            OpExt.NewInfix("/", P_MULTIPLICATIVE_L, P_MULTIPLICATIVE_R, canDivide);
            OpExt.NewInfix("%", P_MULTIPLICATIVE_L, P_MULTIPLICATIVE_L, canModulo);

            OpExt.NewInfix("&", P_BITWISE_AND_L, P_BITWISE_AND_R, canBitwiseAnd);
            OpExt.NewInfix("|", P_BITWISE_OR_L, P_BITWISE_OR_R, canBitwiseOr);
            OpExt.NewInfix("^", P_BITWISE_XOR_L, P_BITWISE_XOR_R, canBitwiseXor);

            OpExt.NewInfix("&&", P_BOOLEAN_AND_L, P_BOOLEAN_AND_R, canBooleanAnd);
            OpExt.NewInfix("||", P_BOOLEAN_OR_L, P_BOOLEAN_OR_R, canBooleanOr);
            OpExt.NewInfix("^^", P_BOOLEAN_XOR_L, P_BOOLEAN_XOR_R, canBooleanXor);

            OpExt.NewInfix("==", P_COMPARISON_L, P_COMPARISON_R, canEquate);
            OpExt.NewInfix("!=", P_COMPARISON_L, P_COMPARISON_R, canInequate);
            OpExt.NewInfix("<", P_COMPARISON_L, P_COMPARISON_R, canBeLess);
            OpExt.NewInfix(">", P_COMPARISON_L, P_COMPARISON_R, canBeGreater);
            OpExt.NewInfix("<=", P_COMPARISON_L, P_COMPARISON_R, canBeLessOrEqual);
            OpExt.NewInfix(">=", P_COMPARISON_L, P_COMPARISON_R, canBeGreaterOrEqual);

            OpExt.NewPrefix("~", P_BITWISE_NOT_R, canNegateBitwise);
            OpExt.NewPrefix("!", P_BOOLEAN_NOT_R, canNegateBoolean);
        }

        public override void InitTransitions()
        {
        }

        public override void InitTypes()
        {
            canAdd = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Float, Z.Float }, Z.Float, ZI.Operator.AddF.CodeMultiFunc2(), OpExt.nInfixOperator },
                { new[] { Z.Int, Z.Int }, Z.Int, ZI.Operator.Add.CodeMultiFunc2(), OpExt.nInfixOperator },
            };

            canUnaryPlus = new OperatorOverloadDict(1)
            {
                { new[] { Z.Float }, Z.Float, storages => ZI.Code.None, OpExt.nPrefixOperator },
                { new[] { Z.Int }, Z.Int, storages => ZI.Code.None, OpExt.nPrefixOperator },
            };

            canSubtract = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Float, Z.Float }, Z.Float, ZI.Operator.SubtractF.CodeMultiFunc2(), OpExt.nInfixOperator },
                { new[] { Z.Int, Z.Int }, Z.Int, ZI.Operator.Subtract.CodeMultiFunc2(), OpExt.nInfixOperator },
            };

            canNegate = new OperatorOverloadDict(1)
            {
                { new[] { Z.Float }, Z.Float, ZI.Operator.NegateF.CodeMultiFunc1(), OpExt.nPrefixOperator },
                { new[] { Z.Int }, Z.Int, ZI.Operator.Negate.CodeMultiFunc1(), OpExt.nPrefixOperator },
            };

            canMultiply = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Float, Z.Float }, Z.Float, ZI.Operator.MultiplyF.CodeMultiFunc2(), OpExt.nInfixOperator },
                { new[] { Z.Int, Z.Int }, Z.Int, ZI.Operator.Multiply.CodeMultiFunc2(), OpExt.nInfixOperator },
            };

            canIntDivide = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Int, Z.Int }, Z.Int, ZI.Operator.IntDivide.CodeMultiFunc2(), OpExt.nInfixOperator }
            };

            canDivide = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Float, Z.Float }, Z.Float, ZI.Operator.DivideF.CodeMultiFunc2(), OpExt.nInfixOperator },
                { new[] { Z.Int, Z.Int }, Z.Float, ZI.Operator.Divide.CodeMultiFunc2(), OpExt.nInfixOperator },
            };

            canModulo = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Float, Z.Float }, Z.Float, ZI.Operator.ModuloF.CodeMultiFunc2(), OpExt.nInfixOperator },
                { new[] { Z.Int, Z.Int }, Z.Int, ZI.Operator.Modulo.CodeMultiFunc2(), OpExt.nInfixOperator }
            };

            canBitwiseAnd = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Int, Z.Int }, Z.Int, ZI.Operator.And.CodeMultiFunc2(), OpExt.nInfixOperator }
            };

            canBitwiseOr = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Int, Z.Int }, Z.Int, ZI.Operator.Or.CodeMultiFunc2(), OpExt.nInfixOperator }
            };

            canBitwiseXor = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Int, Z.Int }, Z.Int, ZI.Operator.Xor.CodeMultiFunc2(), OpExt.nInfixOperator }
            };

            canBooleanAnd = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Bool, Z.Bool }, Z.Bool, storages => throw new NotImplementedException(), OpExt.nInfixOperator },
                // TODO implement short-circuit boolean operators
            };

            canBooleanOr = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Bool, Z.Bool }, Z.Bool, storages => throw new NotImplementedException(), OpExt.nInfixOperator },
            };

            canBooleanXor = new OperatorOverloadDict(0, 2) // only differences between this and bitwise xor are type and precedence
            {
                { new[] { Z.Bool, Z.Bool }, Z.Bool, ZI.Operator.Xor.CodeMultiFunc2(), OpExt.nInfixOperator }
            };

            canEquate = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Float, Z.Float }, Z.Bool, ZI.Operator.EqualF.CodeMultiFunc2(), OpExt.nInfixOperator },
                { new[] { Z.Int, Z.Int }, Z.Bool, ZI.Operator.Equal.CodeMultiFunc2(), OpExt.nInfixOperator }
            };

            canInequate = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Float, Z.Float }, Z.Bool, ZI.Operator.NotEqualF.CodeMultiFunc2(), OpExt.nInfixOperator },
                { new[] { Z.Int, Z.Int }, Z.Bool, ZI.Operator.NotEqual.CodeMultiFunc2() ,OpExt.nInfixOperator }
            };

            canBeLess = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Float, Z.Float }, Z.Bool, ZI.Operator.LessF.CodeMultiFunc2(), OpExt.nInfixOperator },
                { new[] { Z.Int, Z.Int }, Z.Bool, ZI.Operator.Less.CodeMultiFunc2(), OpExt.nInfixOperator }
            };

            canBeGreater = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Float, Z.Float }, Z.Bool, ZI.Operator.GreaterF.CodeMultiFunc2(), OpExt.nInfixOperator },
                { new[] { Z.Int, Z.Int }, Z.Bool, ZI.Operator.Greater.CodeMultiFunc2(), OpExt.nInfixOperator }
            };

            canBeLessOrEqual = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Float, Z.Float }, Z.Bool, ZI.Operator.LessOrEqualF.CodeMultiFunc2(), OpExt.nInfixOperator },
                { new[] { Z.Int, Z.Int }, Z.Bool, ZI.Operator.LessOrEqual.CodeMultiFunc2(), OpExt.nInfixOperator }
            };

            canBeGreaterOrEqual = new OperatorOverloadDict(0, 2)
            {
                { new[] { Z.Float, Z.Float }, Z.Bool, ZI.Operator.GreaterOrEqualF.CodeMultiFunc2(), OpExt.nInfixOperator },
                { new[] { Z.Int, Z.Int }, Z.Bool, ZI.Operator.GreaterOrEqual.CodeMultiFunc2(), OpExt.nInfixOperator }
            };

            canNegateBitwise = new OperatorOverloadDict(1)
            {
                { new[] { Z.Int }, Z.Int, ZI.Operator.BitwiseNegate.CodeMultiFunc1(), OpExt.nPrefixOperator }
            };

            canNegateBoolean = new OperatorOverloadDict(1)
            {
                { new[] { Z.Bool }, Z.Bool, ZI.Operator.BooleanNegate.CodeMultiFunc1(), OpExt.nPrefixOperator }
            };
        }
    }
}
