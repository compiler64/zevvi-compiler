using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZevviCompiler.Punctuation;
using ZevviCompiler.Transitions;

namespace ZevviCompiler.ForLoops
{
    /// <summary>
    /// A Zevvi compiler extension with the syntax and keywords of "for" loops.
    /// Contains the "for" keyword.<br/>
    /// Requirements: <see cref="PunctuationExtension"/>.
    /// </summary>
    public class ForLoopsExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(PunctuationExtension) };

        private PunctuationExtension PuncExt => Z.GetExtension<PunctuationExtension>();

        public StateDict ForDict;

        public StateIndex For, For_LeftParen, For_LeftParen_Normal, For_LeftParen_Normal_Semicolon, For_LeftParen_Normal_Semicolon_Normal;
        public StateIndex For_LeftParen_Normal_Semicolon_Normal_Semicolon, For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal;
        public StateIndex For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen, For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Normal;
        public StateIndex For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Statement;
        public State sFor, sFor_LeftParen, sFor_LeftParen_Normal, sFor_LeftParen_Normal_Semicolon, sFor_LeftParen_Normal_Semicolon_Normal;
        public State sFor_LeftParen_Normal_Semicolon_Normal_Semicolon, sFor_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal;
        public State sFor_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen, sFor_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Normal;
        public State sFor_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Statement;
        public Nonterminal nForLoop;

        public override void InitConverts()
        {
        }

        public override void InitStates()
        {
            nForLoop = new("ForLoop");

            For = new("For");
            For_LeftParen = new("For_LeftParen");
            For_LeftParen_Normal = new("For_LeftParen_Normal");
            For_LeftParen_Normal_Semicolon = new("For_LeftParen_Normal_Semicolon");
            For_LeftParen_Normal_Semicolon_Normal = new("For_LeftParen_Normal_Semicolon_Normal");
            For_LeftParen_Normal_Semicolon_Normal_Semicolon = new("For_LeftParen_Normal_Semicolon_Normal_Semicolon");
            For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal = new("For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal");
            For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen
                = new("For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen");
            For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Normal
                = new("For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Normal");
            For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Statement
                = new("For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Statement");

            sFor = new(For, 1);
            sFor_LeftParen = new(For_LeftParen, 2);
            sFor_LeftParen_Normal = new(For_LeftParen_Normal, 3);
            sFor_LeftParen_Normal_Semicolon = new(For_LeftParen_Normal_Semicolon, 4);
            sFor_LeftParen_Normal_Semicolon_Normal = new(For_LeftParen_Normal_Semicolon_Normal, 5);
            sFor_LeftParen_Normal_Semicolon_Normal_Semicolon = new(For_LeftParen_Normal_Semicolon_Normal_Semicolon, 6);
            sFor_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal = new(For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal, 7);
            sFor_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen
                = new(For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen, 8);
            sFor_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Normal
                = new(For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Normal, 9);
            sFor_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Statement
                = new(For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Statement, 9, ForLoopMerge);
        }

        public override void InitSymbolTable()
        {
            Z.symbolTable.Add("for", ZType.WithVoidConverts(Z, "'for'", new NormalTransition("'for'", ForDict)));
        }

        public override void InitTransitions()
        {
            Z.normalStates.UnionWith(new[]
            {
                For_LeftParen_Normal, For_LeftParen_Normal_Semicolon_Normal, For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal,
                For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Normal,
            });

            Z.operatorStates.UnionWith(new[]
            {
                For_LeftParen, For_LeftParen_Normal_Semicolon, For_LeftParen_Normal_Semicolon_Normal_Semicolon,
                For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen,
            });

            Z.NormalDict.Add(new StateDict(Z)
            {
                { For_LeftParen, sFor_LeftParen_Normal },
                { For_LeftParen_Normal_Semicolon, sFor_LeftParen_Normal_Semicolon_Normal },
                { For_LeftParen_Normal_Semicolon_Normal_Semicolon, sFor_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal },
                { For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen, sFor_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Normal },
            });

            Z.symbolTable.Get(";").type.Transition.StateDict.Add(new StateDict(Z)
            {
                { For_LeftParen_Normal, sFor_LeftParen_Normal_Semicolon },
                { For_LeftParen_Normal_Semicolon_Normal, sFor_LeftParen_Normal_Semicolon_Normal_Semicolon },
            });

            PuncExt.StatementDict.Add(new StateDict(Z)
            {
                { For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen,
                    sFor_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen_Statement },
            });

            PuncExt.LeftParenDict.Add(new StateDict(Z)
            {
                { For, sFor_LeftParen, () => Z.symbolTable.PushScope() }
            });

            PuncExt.RightParenDict.Add(new StateDict(Z)
            {
                { For_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal,
                    sFor_LeftParen_Normal_Semicolon_Normal_Semicolon_Normal_RightParen }
            });

            ForDict = new(Z)
            {
                { Z.operatorStates, sFor }
            };
        }

        public override void InitTypes()
        {
        }

        public ParseTree ForLoopMerge(ParseTree[] subtrees)
        {
            // the parse tree for the initializer statement of the 'for' loop
            ParseTree initializer = subtrees[2];
            // the code, including the convert to void, of the initializer statement
            ZI.Code initializerCode = initializer.ImplicitConvertTree(Z.Void)
                ?? throw new ZevviTypeError($"Type error in 'for' loop initializer statement: Cannot convert from" +
                    $"{initializer.exprType} to {Z.Void}.", initializer.GetTokens());

            // the parse tree for the condition of the 'for' loop
            ParseTree condition = subtrees[4];
            // the code, including the convert to bool, of the condition
            ZI.Code conditionCode = condition.ImplicitConvertTree(Z.Bool)
                ?? throw new ZevviTypeError($"Type error in 'for' loop condition: Cannot convert from" +
                    $"{condition.exprType} to {Z.Bool}.", condition.GetTokens());

            // the parse tree for the updater statement of the 'for' loop
            ParseTree updater = subtrees[6];
            // the code, including the convert to void, of the updater statement
            ZI.Code updaterCode = updater.ImplicitConvertTree(Z.Void)
                ?? throw new ZevviTypeError($"Type error in 'for' loop condition: Cannot convert from" +
                    $"{updater.exprType} to {Z.Void}.", updater.GetTokens());

            // the parse tree for the block of the 'for' loop
            ParseTree block = subtrees[8];
            // the code, including the convert to void, of the block
            ZI.Code blockCode = block.exprCode;

            // a label right before the condition is checked
            ZI.Triple beforeCondition = ZI.Triple.DoNothing;
            // a label right after the loop
            ZI.Triple afterLoop = ZI.Triple.DoNothing;

            // the ZI code
            ZI.Code code = initializerCode + beforeCondition + conditionCode
                + new ZI.Triple(ZI.Operator.IfNot, conditionCode.storage, afterLoop)
                + blockCode + updaterCode
                + new ZI.Triple(ZI.Operator.Goto, beforeCondition)
                + afterLoop;

            // remove the 'for' loop scope
            Z.symbolTable.PopScope();

            // return a new parse tree
            return new ParseTree(subtrees, PuncExt.Statement, code, nForLoop);
        }
    }
}
