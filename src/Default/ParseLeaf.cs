using System;
using System.Collections.Generic;

namespace ZevviCompiler
{
    public class ParseLeaf : ParseTree, IParseLeaf
    {
        public Token Token { get; set; }

        public override bool IsLeaf => true;

        public ParseLeaf(Token token, IType exprType, ZI.Code exprCode, State state) : base(Array.Empty<ParseTree>(), state, exprType, exprCode, Nonterminal.ForToken(token.TokenType))
        {
            Token = token;
        }

        public ParseLeaf(Token token, IType exprType, ZI.Code exprCode) : this(token, exprType, exprCode, State.Null)
        {
        }

        public override string ToString()
        {
            return $"{Token}";
        }

        public override RightPrecLeaf AttachRightPrec(int rightPrec)
        {
            return new RightPrecLeaf(Token, exprType, exprCode, state, rightPrec);
        }

        public override List<Token> GetTokens()
        {
            return new List<Token> { Token };
        }

        public override ParseLeaf Reload()
        {
            return Token.ToParseTree();
        }
    }
}
