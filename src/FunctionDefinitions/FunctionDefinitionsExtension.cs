using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZevviCompiler.FunctionCalls;
using ZevviCompiler.OperatorSyntaxes;
using ZevviCompiler.Punctuation;
using ZevviCompiler.Transitions;
using ZevviCompiler.Variables;

namespace ZevviCompiler.FunctionDefinitions
{
    /// <summary>
    /// A Zevvi compiler extension with function declaration syntax.<br/>
    /// Requirements:
    /// <see cref="OperatorSyntaxesExtension"/>, <see cref="PunctuationExtension"/>,
    /// <see cref="VariablesExtension"/>, <see cref="FunctionCallsExtension"/>.
    /// </summary>
    public class FunctionDefinitionsExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(OperatorSyntaxesExtension), typeof(PunctuationExtension), typeof(VariablesExtension), typeof(FunctionCallsExtension) };

        public OperatorSyntaxesExtension OpExt => Z.GetExtension<OperatorSyntaxesExtension>();

        private PunctuationExtension PuncExt => Z.GetExtension<PunctuationExtension>();

        private VariablesExtension VarExt => Z.GetExtension<VariablesExtension>();

        private FunctionCallsExtension FuncExt => Z.GetExtension<FunctionCallsExtension>();

        public Stack<FunctionInfo> functions = new();

        public ZI.Variable NextLocalVarLoc => new(functions.Peek().NextLocalVarNum++, ZI.VariableStorageType.Local);

        public StateIndex Type_Normal_LeftParen, Type_Normal_LeftParen_ParamList_Type, Type_Normal_LeftParen_ParamList_Type_Normal, Type_Normal_LeftParen_ParamList_Type_Normal_Comma, Type_Normal_LeftParen_ParamListNoComma_RightParen, Type_Normal_LeftParen_ParamListNoComma_RightParen_Normal, Type_Normal_LeftParen_ParamListNoComma_RightParen_Statement;
        public State sType_Normal_LeftParen;
        public Nonterminal nFunctionDefinition, nReturnStatement;

        public const int P_RETURN_R = 4000;

        public State SType_Normal_LeftParen_ParamList_Type(int numToPop)
        {
            return new(Type_Normal_LeftParen_ParamList_Type, numToPop);
        }

        public State SType_Normal_LeftParen_ParamList_Type_Normal(int numToPop)
        {
            return new(Type_Normal_LeftParen_ParamList_Type_Normal, numToPop);
        }

        public State SType_Normal_LeftParen_ParamList_Type_Normal_Comma(int numToPop)
        {
            return new(Type_Normal_LeftParen_ParamList_Type_Normal_Comma, numToPop);
        }

        public State SType_Normal_LeftParen_ParamListNoComma_RightParen(int numToPop)
        {
            return new(Type_Normal_LeftParen_ParamListNoComma_RightParen, numToPop);
        }

        public State SType_Normal_LeftParen_ParamListNoComma_RightParen_Normal(int numToPop)
        {
            return new(Type_Normal_LeftParen_ParamListNoComma_RightParen_Normal, numToPop);
        }

        public State SType_Normal_LeftParen_ParamListNoComma_RightParen_Statement(int numToPop)
        {
            return new(Type_Normal_LeftParen_ParamListNoComma_RightParen_Statement, numToPop, subtrees => FunctionDefinitionMerge(subtrees));
        }

        public override void InitConverts()
        {
        }

        public override void InitStates()
        {
            nFunctionDefinition = new("FunctionDefinition");
            nReturnStatement = new("ReturnStatement");

            Type_Normal_LeftParen = new("Type_Normal_LeftParen");
            Type_Normal_LeftParen_ParamList_Type = new("Type_Normal_LeftParen_ParamList_Type");
            Type_Normal_LeftParen_ParamList_Type_Normal = new("Type_Normal_LeftParen_ParamList_Type_Normal");
            Type_Normal_LeftParen_ParamList_Type_Normal_Comma = new("Type_Normal_LeftParen_ParamList_Type_Normal_Comma");
            Type_Normal_LeftParen_ParamListNoComma_RightParen = new("Type_Normal_LeftParen_ParamListNoComma_RightParen");
            Type_Normal_LeftParen_ParamListNoComma_RightParen_Normal = new("Type_Normal_LeftParen_ParamListNoComma_RightParen_Normal");
            Type_Normal_LeftParen_ParamListNoComma_RightParen_Statement = new("Type_Normal_LeftParen_ParamListNoComma_RightParen_Statement");

            sType_Normal_LeftParen = new(Type_Normal_LeftParen, 3);
        }

        public override void InitSymbolTable()
        {
            Z.symbolTable.Add("return", new OperatorType(Z, "'return'")
            {
                Transition = new RightPrecTransition(P_RETURN_R, "'return'", OpExt.StandaloneOrPrefixDict),
                MergeSubtrees = subtrees => ReturnMerge(subtrees),
            });
        }

        public override void InitTransitions()
        {
            Z.normalStates.Add(Type_Normal_LeftParen_ParamListNoComma_RightParen_Normal);
            Z.operatorStates.Add(Type_Normal_LeftParen_ParamListNoComma_RightParen);

            Z.NormalDict.Add(new StateDict(Z)
            {
                { Type_Normal_LeftParen_ParamList_Type, () => SType_Normal_LeftParen_ParamList_Type_Normal(Z.LastNumToPop + 1) },
                { Type_Normal_LeftParen_ParamListNoComma_RightParen, () => SType_Normal_LeftParen_ParamListNoComma_RightParen_Normal(Z.LastNumToPop + 1) },
            });

            PuncExt.LeftParenDict.Add(new StateDict(Z)
            {
                { VarExt.Type_Normal, sType_Normal_LeftParen },
            });

            PuncExt.RightParenDict.Add(new StateDict(Z)
            {
                { Type_Normal_LeftParen, SType_Normal_LeftParen_ParamListNoComma_RightParen(4), EnterFunction },
                { Type_Normal_LeftParen_ParamList_Type_Normal, () => SType_Normal_LeftParen_ParamListNoComma_RightParen(Z.LastNumToPop + 1), EnterFunction },
            });

            PuncExt.CommaDict.Add(new StateDict(Z)
            {
                { Type_Normal_LeftParen_ParamList_Type_Normal, () => SType_Normal_LeftParen_ParamList_Type_Normal_Comma(Z.LastNumToPop + 1) },
            });

            PuncExt.StatementDict.Add(new StateDict(Z)
            {
                { Type_Normal_LeftParen_ParamListNoComma_RightParen, () => SType_Normal_LeftParen_ParamListNoComma_RightParen_Statement(Z.LastNumToPop + 1) },
            });

            VarExt.TypeDict.Add(new StateDict(Z)
            {
                { Type_Normal_LeftParen, SType_Normal_LeftParen_ParamList_Type(4) },
                { Type_Normal_LeftParen_ParamList_Type_Normal_Comma, () => SType_Normal_LeftParen_ParamList_Type(Z.LastNumToPop + 1) },
            });
        }

        public override void InitTypes()
        {
            VarExt.declarationMergeFuncs.Add(_ => functions.Count > 0 ? subtrees => VarExt.DeclarationMerge(subtrees, NextLocalVarLoc) : null);
        }

        private void AddFunctionToTable(IType[] paramTypes, IType returnType, LinkedListNode<ParseTree> firstNode)
        {
            // the name of the type of the function (e.g. (int, string) -> void)
            string funcTypeName = "(" + string.Join<IType>(", ", paramTypes) + ") -> " + returnType;

            // the function name parse tree
            ParseTree funcNameTree = firstNode.Next.Value;

            // the name of the function
            string funcName = funcNameTree.GetIdentifier(type => $"Expected identifier for function name, got {type}.");

            // the type of the function
            FunctionType funcType = new(FuncExt, funcTypeName, functions.Peek().CallMark,
                FunctionCallsExtension.NormalFunction(paramTypes.ToArray(), returnType));

            /*// the code that is emitted when the function identifier token is read
            ZI.Code funcCode = new SingleMI(MI.PushCon, Z.symbolTable.scopes.Peek().nextVarLoc++);*/

            // add the function to the symbol table
            Z.symbolTable.Add(funcName, funcType);
        }

        private void EnterFunction()
        {
            LinkedListNode<ParseTree> node = Z.subtrees.Last;
            int numToPop = node.Value.state.numToPop;
            int numParams = numToPop == 3 ? 0 : (numToPop - 2) / 3;
            int paramNum = numParams;

            IType[] paramTypes = new IType[numParams];
            IType[] paramVarTypes = new IType[numParams];

            for (int i = 0; i < Z.LastNumToPop; i++)
            {
                State state = node.Value.state;

                if (state.index == Type_Normal_LeftParen_ParamList_Type_Normal)
                {
                    // decrement paramNum (parameters are looped over backwards)
                    paramNum--;
                    // get the type of the parameter from the type keyword subtree
                    IType paramType = node.Previous.Value.exprType.As<TypeType>().innerType;
                    // get the identifier of the parameter from the parameter name subtree
                    string id = node.Value.GetIdentifier(type => $"Expected identifier for parameter name, got {type}.");
                    // create new parameter storage for the parameter
                    ZI.Variable paramStorage = new(paramNum, ZI.VariableStorageType.Parameter);
                    // the variable type 
                    VariableType paramVarType = new(VarExt, id, paramType, paramStorage);
                    paramTypes[paramNum] = paramType;
                    paramVarTypes[paramNum] = paramVarType;
                }

                node = node.Previous;
            }

            node = node.Next;

            /*paramTypes.Reverse();
            paramVarTypes.Reverse();*/
            IType returnType = node.Value.exprType.As<TypeType>().innerType;

            ParseTree funcNameTree = node.Next.Value;

            string funcName = funcNameTree.GetIdentifier(type => $"Expected identifier for function name, got {type}.");

            ZI.Triple callMark = ZI.Triple.DoNothing;
            functions.Push(new FunctionInfo(
                funcName,
                paramTypes.ToArray(),
                returnType,
                callMark
            ));

            // add the function to the symbol table in the scope of the function definition
            AddFunctionToTable(paramTypes, returnType, node);

            // push the scope for the function body
            Z.symbolTable.PushScope();

            for (int i = 0; i < numParams; i++)
            {
                // get the i-th parameter variable type
                VariableType paramVarType = paramVarTypes[i].As<VariableType>();
                // add the i-th parameter to the symbol table
                Z.symbolTable.Add(paramVarType.Identifier, paramVarType, paramVarType.storage);
            }
        }

        public void ExitFunction()
        {
            Z.symbolTable.PopScope();
            functions.Pop();
        }

        /*public ParseTree LocalVariableDeclarationMerge(ParseTree[] subtrees)
        {
            // the type keyword used to declare the variable
            TypeType typeType = subtrees[0].exprType.As<TypeType>();
            // the type that the type keyword represents
            IType innerType = typeType.innerType;
            // the variable name
            string id = subtrees[1].exprType.As<IIdentifierType>().Identifier;
            // the ZI operand that stores the variable
            ZI.Variable storage = NextLocalVarLoc;
            // the variable type
            VariableType varType = new(VarExt, id, innerType, storage);
            // add the variable to the symbol table
            Z.symbolTable.Add(id, varType, storage);
            // return the new parse tree
            return ParseTree.CombineTrees(subtrees, varType, ZI.Code.None);
        }*/

        public ParseTree FunctionDefinitionMerge(ParseTree[] subtrees)
        {
            // the parse tree of the function definition block
            ParseTree block = subtrees[^1];
            // the ZI code of the block
            ZI.Code blockCode = block.exprCode;
            // a label right before the function block
            ZI.Triple beforeFuncBlock = functions.Peek().CallMark;
            // a label right after the function block
            ZI.Triple afterFuncBlock = ZI.Triple.DoNothing;
            // 'goto' statement to skip over the function block at the definition
            ZI.Triple t1 = new(ZI.Operator.Goto, afterFuncBlock);
            // 'return' statement at the end of the function block
            ZI.Triple t2 = new(ZI.Operator.Return);
            // ZI code for the function declaration
            ZI.Code code = t1 + beforeFuncBlock + blockCode + t2 + afterFuncBlock;
            // remove parameter scope and function info
            ExitFunction();
            // return the new parse tree
            return new ParseTree(subtrees, PuncExt.Statement, code, nFunctionDefinition);
        }

        public ParseTree ReturnMerge(ParseTree[] subtrees)
        {
            // the return type of the innermost function definition
            IType returnType = functions.Peek().ReturnType;

            // if the return statement has no return value
            if (subtrees.Length == 1)
            {
                // try to convert void to the return type of the function
                ZI.CodeFunc codeFunc = Z.Void.ImplicitConvert(returnType);

                if (codeFunc is null)
                {
                    throw new ZevviTypeError($"Type error in 'return' statement: Cannot convert from " +
                    $"{Z.Void} to {returnType}.", subtrees[0].GetTokens());
                }

                // the 'return' triple
                ZI.Triple t1 = new(ZI.Operator.Return);

                // return the new parse tree
                return new ParseTree(subtrees, Z.Void, codeFunc(null) + t1, nReturnStatement);
            }
            else
            {
                // the tree that follows the 'return' keyword
                ParseTree returnedTree = subtrees[1];

                // try to convert the returned tree to the return type of the function
                ZI.Code convert = returnedTree.ImplicitConvertTree(returnType);

                // if the expression to return has invalid type then throw an error
                if (convert is null)
                {
                    throw new ZevviTypeError($"Type error in 'return' statement: Cannot convert from " +
                        $"{returnedTree.exprType} to {returnType}.", returnedTree.GetTokens());
                }

                // the 'return' triple
                ZI.Triple t1 = new(ZI.Operator.Return, convert.storage);

                // return the new parse tree
                return new ParseTree(subtrees, Z.Void, convert + t1, nReturnStatement);
            }
        }
    }
}
