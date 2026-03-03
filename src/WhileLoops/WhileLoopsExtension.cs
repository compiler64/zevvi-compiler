using System;
using System.Collections.Generic;
using System.Text;
using ZevviCompiler.Punctuation;
using ZevviCompiler.Transitions;

namespace ZevviCompiler.WhileLoops
{
    /// <summary>
    /// A Zevvi compiler extension with the syntax and keywords of "while" and "do-while" loops.
    /// Contains the 'while' and 'do' keywords.<br/>
    /// Requirements: <see cref="PunctuationExtension"/>.
    /// </summary>
    public class WhileLoopsExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(PunctuationExtension) };

        private PunctuationExtension PuncExt => Z.GetExtension<PunctuationExtension>();

        public StateDict WhileDict, DoDict;

        public StateIndex While, While_LeftParen, While_LeftParen_Normal, While_LeftParen_Normal_RightParen, While_LeftParen_Normal_RightParen_Normal, While_LeftParen_Normal_RightParen_Statement;
        public StateIndex Do, Do_Normal, Do_Statement, Do_Statement_While, Do_Statement_While_LeftParen, Do_Statement_While_LeftParen_Normal, Do_Statement_While_LeftParen_Normal_RightParen;
        public State sWhile, sWhile_LeftParen, sWhile_LeftParen_Normal, sWhile_LeftParen_Normal_RightParen, sWhile_LeftParen_Normal_RightParen_Normal, sWhile_LeftParen_Normal_RightParen_Statement;
        public State sDo, sDo_Normal, sDo_Statement, sDo_Statement_While, sDo_Statement_While_LeftParen, sDo_Statement_While_LeftParen_Normal, sDo_Statement_While_LeftParen_Normal_RightParen;
        public Nonterminal nWhileLoop, nDoWhileLoop;

        public override void InitConverts()
        {
        }

        public override void InitStates()
        {
            nWhileLoop = new("WhileLoop");
            nDoWhileLoop = new("DoWhileLoop");

            While = new("While");
            While_LeftParen = new("While_LeftParen");
            While_LeftParen_Normal = new("While_LeftParen_Normal");
            While_LeftParen_Normal_RightParen = new("While_LeftParen_Normal_RightParen");
            While_LeftParen_Normal_RightParen_Normal = new("While_LeftParen_Normal_RightParen_Normal");
            While_LeftParen_Normal_RightParen_Statement = new("While_LeftParen_Normal_RightParen_Statement");
            Do = new("Do");
            Do_Normal = new("Do_Normal");
            Do_Statement = new("Do_Statement");
            Do_Statement_While = new("Do_Statement_While");
            Do_Statement_While_LeftParen = new("Do_Statement_While_LeftParen");
            Do_Statement_While_LeftParen_Normal = new("Do_Statement_While_LeftParen_Normal");
            Do_Statement_While_LeftParen_Normal_RightParen = new("Do_Statement_While_LeftParen_Normal_RightParen");

            sWhile = new(While, 1);
            sWhile_LeftParen = new(While_LeftParen, 2);
            sWhile_LeftParen_Normal = new(While_LeftParen_Normal, 3);
            sWhile_LeftParen_Normal_RightParen = new(While_LeftParen_Normal_RightParen, 4);
            sWhile_LeftParen_Normal_RightParen_Normal = new(While_LeftParen_Normal_RightParen_Normal, 5);
            sWhile_LeftParen_Normal_RightParen_Statement = new(While_LeftParen_Normal_RightParen_Statement, 5, subtrees => WhileMerge(subtrees));
            sDo = new(Do, 1);
            sDo_Normal = new(Do_Normal, 2);
            sDo_Statement = new(Do_Statement, 2);
            sDo_Statement_While = new(Do_Statement_While, 3);
            sDo_Statement_While_LeftParen = new(Do_Statement_While_LeftParen, 4);
            sDo_Statement_While_LeftParen_Normal = new(Do_Statement_While_LeftParen_Normal, 5);
            sDo_Statement_While_LeftParen_Normal_RightParen = new(Do_Statement_While_LeftParen_Normal_RightParen, 6, subtrees => DoWhileMerge(subtrees));
        }

        public override void InitSymbolTable()
        {
            Z.symbolTable.Add("while", new ZType(Z, "'while'") { Transition = new NormalTransition("'while'", WhileDict) });
            Z.symbolTable.Add("do", new ZType(Z, "'do'") { Transition = new NormalTransition("'do'", DoDict) });
        }

        public override void InitTransitions()
        {
            Z.normalStates.UnionWith(new[]
            {
                While_LeftParen_Normal, While_LeftParen_Normal_RightParen_Normal, Do_Normal,
                Do_Statement_While_LeftParen_Normal
            });

            Z.operatorStates.UnionWith(new[]
            {
                While_LeftParen, While_LeftParen_Normal_RightParen, Do, Do_Statement_While_LeftParen
            });

            Z.NormalDict.Add(new StateDict(Z)
            {
                { While_LeftParen, sWhile_LeftParen_Normal },
                { While_LeftParen_Normal_RightParen, sWhile_LeftParen_Normal_RightParen_Normal },
                { Do, sDo_Normal },
                { Do_Statement_While_LeftParen, sDo_Statement_While_LeftParen_Normal }
            });

            PuncExt.StatementDict.Add(new StateDict(Z)
            {
                { While_LeftParen_Normal_RightParen, sWhile_LeftParen_Normal_RightParen_Statement },
                { Do, sDo_Statement }
            });

            PuncExt.LeftParenDict.Add(new StateDict(Z)
            {
                { While, sWhile_LeftParen },
                { Do_Statement_While, sDo_Statement_While_LeftParen }
            });
            
            PuncExt.RightParenDict.Add(new StateDict(Z)
            {
                { While_LeftParen_Normal, sWhile_LeftParen_Normal_RightParen },
                { Do_Statement_While_LeftParen_Normal, sDo_Statement_While_LeftParen_Normal_RightParen }
            });

            WhileDict = new StateDict(Z)
            {
                { Do_Statement, sDo_Statement_While },
                { Z.operatorStates, sWhile }
            };

            DoDict = new StateDict(Z)
            {
                { Z.operatorStates, sDo }
            };
        }

        public override void InitTypes()
        {
        }
        
        public ParseTree WhileMerge(ParseTree[] subtrees)
        {
            ParseTree condition = subtrees[2];
            ZI.Code conditionCode = condition.ImplicitConvertTree(Z.Bool);

            if (conditionCode is null)
            {
                throw new ZevviTypeError($"Type error in 'while' condition: Cannot convert from " +
                    $"{condition.exprType} to {Z.Bool}.", condition.GetTokens());
            }

            ParseTree block = subtrees[4];
            ZI.Code blockCode = block.ImplicitConvertTree(Z.Void);

            if (blockCode is null)
            {
                throw new ZevviTypeError($"Type error in 'do' block: Cannot convert from " +
                    $"{block.exprType} to {Z.Void}.", block.GetTokens());
            }

            ZI.Triple afterLoop = ZI.Triple.DoNothing;
            ZI.Code code = conditionCode
                + new ZI.Triple(ZI.Operator.IfNot, conditionCode.storage, afterLoop)
                + blockCode
                + new ZI.Triple(ZI.Operator.Goto, conditionCode.Start)
                + afterLoop;
            return new ParseTree(subtrees, PuncExt.Statement, code, nWhileLoop);
        }

        public ParseTree DoWhileMerge(ParseTree[] subtrees)
        {
            ParseTree condition = subtrees[4];
            ZI.Code conditionCode = condition.ImplicitConvertTree(Z.Bool);

            if (conditionCode is null)
            {
                throw new ZevviTypeError($"Type error in 'while' condition: Cannot convert from " +
                    $"{condition.exprType} to {Z.Bool}.", condition.GetTokens());
            }

            ParseTree block = subtrees[1];
            ZI.Code blockCode = block.ImplicitConvertTree(Z.Void);

            if (blockCode is null)
            {
                throw new ZevviTypeError($"Type error in 'do' block: Cannot convert from " +
                    $"{block.exprType} to {Z.Void}.", block.GetTokens());
            }

            ZI.Code code = blockCode
                + conditionCode
                + new ZI.Triple(ZI.Operator.If, conditionCode.storage, blockCode.Start);
            return new ParseTree(subtrees, PuncExt.Statement, code, nDoWhileLoop);
        }
    }
}
