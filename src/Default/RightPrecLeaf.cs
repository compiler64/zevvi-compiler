using System;

namespace ZevviCompiler
{
    public class RightPrecLeaf : RightPrecTree, IParseLeaf
    {
        public Token Token { get; set; }

        public override bool IsLeaf => true;

        public RightPrecLeaf(Token token, IType exprType, ZI.Code exprCode, State state, int rightPrec)
            : base(Array.Empty<ParseTree>(), exprType, exprCode, state, Nonterminal.ForToken(token.TokenType), rightPrec)
        {
            Token = token;
        }

        public RightPrecLeaf(Token token, IType exprType, ZI.Code exprCode, int rightPrec) : this(token, exprType, exprCode, State.Null, rightPrec)
        {
        }

        public override string ToString()
        {
            return Token.ToString();
        }

        public override ParseLeaf Reload()
        {
            return Token.ToParseTree();
        }
    }
}
