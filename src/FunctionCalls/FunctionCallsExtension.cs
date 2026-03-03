using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZevviCompiler.OperatorSyntaxes;
using ZevviCompiler.Punctuation;

namespace ZevviCompiler.FunctionCalls
{
    /// <summary>
    /// A Zevvi compiler extension with function call syntax.<br/>
    /// Requirements: <see cref="OperatorSyntaxesExtension"/>, <see cref="PunctuationExtension"/>.
    /// </summary>
    public class FunctionCallsExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(OperatorSyntaxesExtension), typeof(PunctuationExtension) };

        private OperatorSyntaxesExtension OpExt => Z.GetExtension<OperatorSyntaxesExtension>();

        private PunctuationExtension PuncExt => Z.GetExtension<PunctuationExtension>();

        public StateDict FunctionDict;

        public StateIndex Function, Function_LeftParen, Function_LeftParen_List, Function_LeftParen_List_Comma, Function_LeftParen_List_RightParen;
        public State sFunction, sFunction_LeftParen;
        public Nonterminal nFunctionCall;

        public State SFunction_LeftParen_List(int numToPop)
        {
            return new(Function_LeftParen_List, numToPop);
        }

        public State SFunction_LeftParen_List_Comma(int numToPop)
        {
            return new(Function_LeftParen_List_Comma, numToPop);
        }

        public State SFunction_LeftParen_List_RightParen(int numToPop)
        {
            return new(Function_LeftParen_List_RightParen, numToPop, OpExt.OpTypeMerge(0));
        }

        public override void InitConverts()
        {
        }

        public override void InitStates()
        {
            nFunctionCall = new("FunctionCall");

            Function = new("Function");
            Function_LeftParen = new("Function_LeftParen");
            Function_LeftParen_List = new("Function_LeftParen_List");
            Function_LeftParen_List_Comma = new("Function_LeftParen_List_Comma");
            Function_LeftParen_List_RightParen = new("Function_LeftParen_List_RightParen");

            sFunction = new(Function, 1);
            sFunction_LeftParen = new(Function_LeftParen, 2);
        }

        public override void InitSymbolTable()
        {
        }

        public override void InitTransitions()
        {
            Z.normalStates.Add(Function_LeftParen_List);
            Z.operatorStates.UnionWith(new[] { Function_LeftParen_List_Comma, Function_LeftParen });

            Z.NormalDict.Add(new StateDict(Z)
            {
                { Function_LeftParen, SFunction_LeftParen_List(3) },
                { Function_LeftParen_List_Comma, () => SFunction_LeftParen_List(Z.LastNumToPop + 1) },
            });

            PuncExt.LeftParenDict.Add(new StateDict(Z)
            {
                { Function, sFunction_LeftParen }
            });

            PuncExt.RightParenDict.Add(new StateDict(Z)
            {
                { Function_LeftParen, SFunction_LeftParen_List_RightParen(3) },
                { Function_LeftParen_List, () => SFunction_LeftParen_List_RightParen(Z.LastNumToPop + 1) },
            });

            PuncExt.CommaDict.Add(new StateDict(Z)
            {
                { Function_LeftParen_List, () => SFunction_LeftParen_List_Comma(Z.LastNumToPop + 1) },
            });

            FunctionDict = new StateDict(Z)
            {
                { Z.operatorStates, sFunction }
            };
        }

        public override void InitTypes()
        {
        }

        public static GetReturnTypeFunc NormalFunction(IType[] paramTypes, IType returnType)
        {
            return argTypes =>
            {
                if (argTypes.Length != paramTypes.Length)
                {
                    return default;
                }

                ZI.CodeFunc[] converts = new ZI.CodeFunc[paramTypes.Length];

                for (int i = 0; i < paramTypes.Length; i++)
                {
                    ZI.CodeFunc convert = argTypes[i].ImplicitConvert(paramTypes[i]);

                    if (convert is null)
                    {
                        return default;
                    }
                    else
                    {
                        converts[i] = convert;
                    }
                }

                return new FunctionReturn(returnType, converts);

                // return argTypes.SequenceEqual(paramTypes) ? (returnType, code) : (null, ZI.Code.None);
            };
        }

        public static GetReturnTypeFunc OverloadedFunction(params GetReturnTypeFunc[] overloads)
        {
            return argTypes =>
            {
                foreach (GetReturnTypeFunc overload in overloads)
                {
                    var tuple = overload(argTypes);

                    if (tuple.type is not null) return tuple;
                }

                return default;
            };
        }

        public ParseTree FunctionCallMerge(ParseTree[] subtrees, ZI.Triple codeToCall, GetReturnTypeFunc getReturnType)
        {
            // true if the function call has no arguments
            bool zeroArgs = subtrees.Length == 3;
            // the subtrees of the function call arguments
            ParseTree[] args = zeroArgs ? Array.Empty<ParseTree>()
                : subtrees.Where((_, index) => index % 2 == 0 && index >= 2).ToArray();
            // the number of arguments
            int numArgs = args.Length;
            // the types of the arguments
            IType[] argTypes = args.Select(item => item.exprType).ToArray();
            // get the return type of the function
            (IType returnType, ZI.CodeFunc[] converts) = getReturnType(argTypes);

            // if the argument types were invalid then throw an error
            if (returnType is null)
            {
                // use a special error message if zero arguments
                if (zeroArgs)
                {
                    throw new ZevviTypeError("Wrong argument types for function: <no arguments>.", subtrees[0].GetTokens());
                }
                else
                {
                    throw new ZevviTypeError("Wrong argument types for function: " +
                        $"{string.Join<IType>(", ", argTypes)}.", subtrees.GetTokensRange(2, subtrees.Length - 2));
                }
            }

            // the ZI code for the new parse tree
            ZI.Code code = ZI.Code.None;

            // for each function call argument
            for (int i = 0; i < numArgs; i++)
            {
                // convert the function call argument
                ZI.Code argConvert = subtrees[2 * i + 2].ConvertedWith(converts[i]);
                // add the convert (with the code) and a 'param' triple to code
                code += argConvert + new ZI.Triple(ZI.Operator.Param, argConvert.storage);
            }

            // add the 'call' triple to code
            code += new ZI.Triple(ZI.Operator.Call, codeToCall, new ZI.Constant<int>(argTypes.Length));

            // return the new parse tree
            return new ParseTree(subtrees, returnType, code, nFunctionCall);
        }
    }
}
