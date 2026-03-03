using System;
using System.Collections.Generic;
using System.Text;
using ZevviCompiler.OperatorSyntaxes;
using ZevviCompiler.Transitions;

namespace ZevviCompiler.Punctuation
{
    /// <summary>
    /// A Zevvi compiler extension with the semicolon, the comma, parentheses, and curly braces.
    /// The comma is just a symbol table entry and an empty <see cref="StateDict"/>;
    /// syntax for the comma is added in other extensions.<br/>
    /// Requirements: <see cref="OperatorSyntaxesExtension"/>.
    /// </summary>
    public class PunctuationExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(OperatorSyntaxesExtension) };

        private OperatorSyntaxesExtension OpExt => Z.GetExtension<OperatorSyntaxesExtension>();

        public const int P_SEMICOLON_L = 2000;
        public const int P_PERIOD_L = 20_000;
        public const int P_PERIOD_R = 20_001;

        public OperatorOverloadDict canAccessMember;

        public ZType Statement;
        
        public StateDict LeftParenDict, RightParenDict, LeftBraceDict, RightBraceDict, StatementDict, CommaDict;

        public StateIndex LeftParen, LeftParen_Normal, LeftParen_Normal_RightParen, LeftBrace, LeftBrace_Normal, LeftBrace_Normal_RightBrace;
        public StateIndex Statements, Statements_Normal;
        public State sLeftParen, sLeftParen_Normal, sLeftParen_Normal_RightParen, sLeftBrace, sLeftBrace_Normal, sLeftBrace_Normal_RightBrace;
        public Nonterminal nParenExpression, nBlock, nStatement;

        public HashSet<StateIndex> expectingStatementStates;

        public State SStatements(int numToPop)
        {
            return new(Statements, numToPop, subtrees => StatementsMerge(subtrees));
        }
        
        public State SStatements_Normal(int numToPop)
        {
            return new(Statements_Normal, numToPop);
        }

        public override void InitConverts()
        {
        }

        public override void InitStates()
        {
            nParenExpression = new("ParenExpression");
            nBlock = new("Block");
            nStatement = new("Statement");

            LeftParen = new("LeftParen");
            LeftParen_Normal = new("LeftParen_Normal");
            LeftParen_Normal_RightParen = new("LeftParen_Normal_RightParen");
            LeftBrace = new("LeftBrace");
            LeftBrace_Normal = new("LeftBrace_Normal");
            LeftBrace_Normal_RightBrace = new("LeftBrace_Normal_RightBrace");
            Statements = new("Statements");
            Statements_Normal = new("Statements_Normal");

            sLeftParen = new(LeftParen, 1);
            sLeftParen_Normal = new(LeftParen_Normal, 2);
            sLeftParen_Normal_RightParen = new(LeftParen_Normal_RightParen, 3, subtrees => ParenthesesMerge(subtrees));
            sLeftBrace = new(LeftBrace, 1);
            sLeftBrace_Normal = new(LeftBrace_Normal, 2);
            sLeftBrace_Normal_RightBrace = new(LeftBrace_Normal_RightBrace, 3, subtrees => BracesMerge(subtrees));
        }

        public override void InitSymbolTable()
        {
            Z.symbolTable.Add("(", ZType.WithVoidConverts(Z, "'('", new NormalTransition("'('", LeftParenDict)));
            Z.symbolTable.Add(")", ZType.WithVoidConverts(Z, "')'", new NormalTransition("')'", RightParenDict)));

            Z.symbolTable.Add("{", ZType.WithVoidConverts(Z, "'{'", new NormalTransition("'{'", LeftBraceDict)));
            Z.symbolTable.Add("}", ZType.WithVoidConverts(Z, "'}'", new NormalTransition("'}'", RightBraceDict)));

            Z.symbolTable.Add(",", ZType.WithVoidConverts(Z, "','", new NormalTransition("','", CommaDict)));
            Z.symbolTable.Add(";", new OperatorType(Z, "';'")
            {
                Transition = new LeftPrecTransition(P_SEMICOLON_L, "';'", OpExt.PostfixDict),
                MergeSubtrees = subtrees => ParseTree.CombineTrees(subtrees, new HashSet<int> { 0 },
                    new ZType[] { Z.Void }, Statement, storages => ZI.Code.None, nStatement)
            });

            OpExt.NewInfix(".", P_PERIOD_L, P_PERIOD_R, canAccessMember);
        }

        public override void InitTransitions()
        {
            Z.normalStates.UnionWith(new[] { LeftParen_Normal, LeftBrace_Normal, Statements_Normal });
            Z.operatorStates.UnionWith(new[] { LeftParen, LeftBrace, Statements });

            expectingStatementStates = new HashSet<StateIndex> { Z.Initial, Statements, LeftBrace };

            Z.NormalDict.Add(new StateDict(Z)
            {
                { LeftParen, sLeftParen_Normal },
                { LeftBrace, sLeftBrace_Normal },
                { Statements, () => SStatements_Normal(Z.LastNumToPop + 1) }
            });

            LeftParenDict = new StateDict(Z)
            {
                { Z.operatorStates, sLeftParen }
            };

            RightParenDict = new StateDict(Z)
            {
                { LeftParen_Normal, sLeftParen_Normal_RightParen }
            };

            LeftBraceDict = new StateDict(Z)
            {
                { Z.operatorStates, () => { Z.symbolTable.PushScope(); return sLeftBrace; } }
            };

            RightBraceDict = new StateDict(Z)
            {
                { LeftBrace_Normal, () => { Z.symbolTable.PopScope(); return sLeftBrace_Normal_RightBrace; } }
            };

            StatementDict = new StateDict(Z)
            {
                { Z.Initial, SStatements(1) },
                { LeftBrace, SStatements(1) },
                { Statements, () => SStatements(Z.LastNumToPop + 1) }
            };

            CommaDict = new StateDict(Z); // CommaDict is an empty StateDict
        }

        public override void InitTypes()
        {
            Statement = ZType.WithVoidConverts(Z, "Statement", new NormalTransition("Statement", StatementDict));

            canAccessMember = new OperatorOverloadDict(0, 2);
        }

        public override void InitOther()
        {
            OpExt.OnInfixSecondArgumentParsed += CheckForMemberAccess;
        }

        private void CheckForMemberAccess()
        {
            if (Z.PeekTree.exprType == Z.symbolTable.Get(".").type)
            {
                Z.OnNextTransitionComplete += Z.Merge;
            }
        }

        public ParseTree ParenthesesMerge(ParseTree[] subtrees)
        {
            return ParseTree.CombineTrees(subtrees, subtrees[1].exprType, ZI.Code.None, nParenExpression);
        }

        public ParseTree BracesMerge(ParseTree[] subtrees)
        {
            return ParseTree.CombineTrees(subtrees, Statement, ZI.Code.None, nBlock);
        }

        public ParseTree StatementsMerge(ParseTree[] subtrees)
        {
            return ParseTree.CombineTrees(subtrees, Z.Void, ZI.Code.None, nStatement);
        }
    }
}
