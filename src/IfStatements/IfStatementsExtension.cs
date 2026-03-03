using System;
using System.Collections.Generic;
using System.Text;
using ZevviCompiler.Punctuation;
using ZevviCompiler.Transitions;
//using MI = ZevviCompiler.MachineInstruction;

namespace ZevviCompiler.IfStatements
{
    /// <summary>
    /// A Zevvi compiler extension with the syntax and keywords of "if" statements.
    /// Contains the 'if', 'then', and 'else' keywords.<br/>
    /// Requirements: <see cref="PunctuationExtension"/>.
    /// </summary>
    public class IfStatementsExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(PunctuationExtension) };

        private PunctuationExtension PuncExt => Z.GetExtension<PunctuationExtension>();

        public StateDict IfDict, ElseDict;

        public StateIndex If, If_LeftParen, If_LeftParen_Normal, If_LeftParen_Normal_RightParen, If_LeftParen_Normal_RightParen_Normal, If_LeftParen_Normal_RightParen_Statement;
        public StateIndex If_LeftParen_Normal_RightParen_Statement_Else, If_LeftParen_Normal_RightParen_Statement_Else_Normal, If_LeftParen_Normal_RightParen_Statement_Else_Statement;
        public State sIf, sIf_LeftParen, sIf_LeftParen_Normal, sIf_LeftParen_Normal_RightParen, sIf_LeftParen_Normal_RightParen_Normal, sIf_LeftParen_Normal_RightParen_Statement;
        public State sIf_LeftParen_Normal_RightParen_Statement_Else, sIf_LeftParen_Normal_RightParen_Statement_Else_Normal, sIf_LeftParen_Normal_RightParen_Statement_Else_Statement;
        public Nonterminal nIfStatement;

        public override void InitConverts()
        {
        }

        public override void InitStates()
        {
            nIfStatement = new("IfStatement");

            If = new("If");
            If_LeftParen = new("If_LeftParen");
            If_LeftParen_Normal = new("If_LeftParen_Normal");
            If_LeftParen_Normal_RightParen = new("If_LeftParen_Normal_RightParen");
            If_LeftParen_Normal_RightParen_Normal = new("If_LeftParen_Normal_RightParen_Normal");
            If_LeftParen_Normal_RightParen_Statement = new("If_LeftParen_Normal_RightParen_Statement");
            If_LeftParen_Normal_RightParen_Statement_Else = new("If_LeftParen_Normal_RightParen_Statement_Else");
            If_LeftParen_Normal_RightParen_Statement_Else_Normal = new("If_LeftParen_Normal_RightParen_Statement_Normal");
            If_LeftParen_Normal_RightParen_Statement_Else_Statement = new("If_LeftParen_Normal_RightParen_Statement_Else_Statement");

            sIf = new(If, 1);
            sIf_LeftParen = new(If_LeftParen, 2);
            sIf_LeftParen_Normal = new(If_LeftParen_Normal, 3);
            sIf_LeftParen_Normal_RightParen = new(If_LeftParen_Normal_RightParen, 4);
            sIf_LeftParen_Normal_RightParen_Normal = new(If_LeftParen_Normal_RightParen_Normal, 5);
            sIf_LeftParen_Normal_RightParen_Statement = new(If_LeftParen_Normal_RightParen_Statement, 5, subtrees => IfThenMerge(subtrees));
            sIf_LeftParen_Normal_RightParen_Statement_Else = new(If_LeftParen_Normal_RightParen_Statement_Else, 6);
            sIf_LeftParen_Normal_RightParen_Statement_Else_Normal = new(If_LeftParen_Normal_RightParen_Statement_Else_Normal, 7);
            sIf_LeftParen_Normal_RightParen_Statement_Else_Statement = new(If_LeftParen_Normal_RightParen_Statement_Else_Statement, 7, subtrees => IfThenElseMerge(subtrees));
        }

        public override void InitSymbolTable()
        {
            Z.symbolTable.Add("if", ZType.WithVoidConverts(Z, "'if'", new NormalTransition("'if'", IfDict)));
            // Z.symbolTable.Add("then", ZType.WithDefaultConverts(Z, "'then'", new NormalTransition("'then'", ThenDict)));
            Z.symbolTable.Add("else", ZType.WithVoidConverts(Z, "'else'", new NormalTransition("'else'", ElseDict)));
        }

        public override void InitTransitions()
        {
            Z.normalStates.UnionWith(new[]
            {
                If_LeftParen_Normal, If_LeftParen_Normal_RightParen_Normal,
                If_LeftParen_Normal_RightParen_Statement_Else_Normal
            });

            Z.operatorStates.UnionWith(new[]
            {
                If_LeftParen, If_LeftParen_Normal_RightParen, If_LeftParen_Normal_RightParen_Statement_Else
            });

            Z.NormalDict.Add(new StateDict(Z)
            {
                { If_LeftParen, sIf_LeftParen_Normal },
                { If_LeftParen_Normal_RightParen, sIf_LeftParen_Normal_RightParen_Normal },
                { If_LeftParen_Normal_RightParen_Statement_Else, sIf_LeftParen_Normal_RightParen_Statement_Else_Normal }
            });

            PuncExt.StatementDict.Add(new StateDict(Z)
            {
                { If_LeftParen_Normal_RightParen, sIf_LeftParen_Normal_RightParen_Statement },
                { If_LeftParen_Normal_RightParen_Statement_Else, sIf_LeftParen_Normal_RightParen_Statement_Else_Statement }
            });

            PuncExt.LeftParenDict.Add(If, sIf_LeftParen);
            PuncExt.RightParenDict.Add(If_LeftParen_Normal, sIf_LeftParen_Normal_RightParen);

            IfDict = new StateDict(Z)
            {
                { Z.operatorStates, sIf }
            };
            //ThenDict = new StateDict(Z)
            //{
            //    { If_Normal, sIf_Normal_Then }
            //};
            ElseDict = new StateDict(Z)
            {
                { If_LeftParen_Normal_RightParen_Statement, sIf_LeftParen_Normal_RightParen_Statement_Else }
            };
        }

        public override void InitTypes()
        {
        }

        public ParseTree IfThenMerge(ParseTree[] subtrees)
        {
            // the parse tree for the 'if' statement condition
            ParseTree condition = subtrees[2];
            // the code, including the convert to bool, of the condition
            ZI.Code conditionCode = condition.ImplicitConvertTree(Z.Bool)
                ?? throw new ZevviTypeError($"Type error in 'if' condition: Cannot convert from "
                    + $"{condition.exprType} to {Z.Bool}.", condition.GetTokens());

            // the parse tree for the 'then' block of the 'if' statement
            ParseTree thenBlock = subtrees[4];
            // the code, including the convert to void, of the 'then' block
            ZI.Code thenBlockCode = thenBlock.ImplicitConvertTree(Z.Void)
                ?? throw new ZevviTypeError($"Type error in 'then' block: Cannot convert from "
                    + $"{thenBlock.exprType} to {Z.Void}.", thenBlock.GetTokens());

            // a label right after the 'then' block
            ZI.Triple afterThen = ZI.Triple.DoNothing;
            // the ZI code of the 'if' statement
            ZI.Code code = conditionCode
                + new ZI.Triple(ZI.Operator.IfNot, conditionCode.storage, afterThen)
                + thenBlockCode
                + afterThen;
            // return the parse tree
            return new ParseTree(subtrees, PuncExt.Statement, code, nIfStatement);
        }

        public ParseTree IfThenElseMerge(ParseTree[] subtrees)
        {
            // the parse tree for the 'if' statement condition
            ParseTree condition = subtrees[2];
            // the code, including the convert to bool, of the condition
            ZI.Code conditionCode = condition.ImplicitConvertTree(Z.Bool)
                ?? throw new ZevviTypeError($"Type error in 'if' condition: Cannot convert from " +
                    $"{condition.exprType} to {Z.Bool}.", condition.GetTokens());

            // the parse tree for the 'then' block of the 'if' statement
            ParseTree thenBlock = subtrees[4];
            // the code, including the convert to void, of the 'then' block
            ZI.Code thenBlockCode = thenBlock.exprCode;
                /*thenBlock.ImplicitConvertTree(Z.Void)
                ?? throw new ZevviTypeError($"Type error in 'then' block: Cannot convert from " +
                    $"{thenBlock.exprType} to {Z.Void}.", thenBlock.GetTokens());*/

            // the parse tree for the 'else' block of the 'if' statement
            ParseTree elseBlock = subtrees[6];
            // the code, including the convert to void, of the 'else' block
            ZI.Code elseBlockCode = elseBlock.exprCode;
                /*elseBlock.ImplicitConvertTree(Z.Void)
                ?? throw new ZevviTypeError($"Type error in 'else' block: Cannot convert from " +
                    $"{thenBlock.exprType} to {Z.Void}.", elseBlock.GetTokens());*/

            // a label right after the 'else' block
            ZI.Triple afterElse = ZI.Triple.DoNothing;
            // the ZI code of the 'if' statement
            ZI.Code code = conditionCode
                + new ZI.Triple(ZI.Operator.IfNot, conditionCode.storage, elseBlockCode.Start)
                + thenBlockCode
                + new ZI.Triple(ZI.Operator.Goto, afterElse)
                + elseBlockCode
                + afterElse;
            // return the parse tree
            return new ParseTree(subtrees, PuncExt.Statement, code, nIfStatement);
        }
    }
}
