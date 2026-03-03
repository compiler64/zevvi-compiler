using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZevviCompiler.Punctuation;
using ZevviCompiler.Transitions;

namespace ZevviCompiler.CompileTime
{
    /// <summary>
    /// A Zevvi compiler extension with compile-time code syntax.<br/>
    /// Required extensions: <see cref="PunctuationExtension"/>.
    /// </summary>
    public class CompileTimeExtension : CompilerExtension
    {
        public override ISet<Type> RequiredExtensions => new HashSet<Type> { typeof(PunctuationExtension) };

        private PunctuationExtension PuncExt => Z.GetExtension<PunctuationExtension>();

        public const int P_HASH_R = 1000;

        public StateIndex Hash, Hash_Normal, Hash_Statement;
        public State sHash, sHash_Normal, sHash_Statement;
        public Nonterminal nCompileTimeStatement;

        public StateDict HashDict;

        public override void InitConverts()
        {
        }

        public override void InitStates()
        {
            nCompileTimeStatement = new("CompileTimeStatement");

            Hash = new("Hash");
            Hash_Normal = new("Hash_Normal");
            Hash_Statement = new("Hash_Statement");

            sHash = new(Hash, 1);
            sHash_Normal = new(Hash_Normal, 2);
            sHash_Statement = new(Hash_Statement, 2, CompileTimeStatementMerge);
        }

        public override void InitSymbolTable()
        {
            Z.symbolTable.Add("#", ZType.WithVoidConverts(Z, "'#'", new RightPrecTransition(P_HASH_R, "'#'", HashDict)));
        }

        public override void InitTransitions()
        {
            Z.normalStates.Add(Hash_Normal);
            Z.operatorStates.Add(Hash);
            PuncExt.expectingStatementStates.Add(Hash);

            Z.NormalDict.Add(Hash, sHash_Normal);
            PuncExt.StatementDict.Add(Hash, sHash_Statement);

            HashDict = new(Z)
            {
                { PuncExt.expectingStatementStates, sHash },
            };
        }

        public override void InitTypes()
        {
        }

        public ParseTree CompileTimeStatementMerge(ParseTree[] subtrees)
        {
            // the code to be interpreted at compile-time
            ZI.Code code = subtrees[1].exprCode;
            // interpret the ZI code
            ZI.Interpreter.Interpret(code, Z);
            // return a new parse tree
            return new ParseTree(subtrees, PuncExt.Statement, ZI.Code.None, nCompileTimeStatement);
        }
    }
}
