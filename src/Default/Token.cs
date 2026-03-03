using MyLibraries.LexicalAnalyzer;
using System;
using System.Collections.Generic;

namespace ZevviCompiler
{
    public sealed class Token : AbstractToken<TokenType>
    {
        public readonly DefaultExtension z;

        public Token(DefaultExtension z, TokenType tokenType, string lexeme, PositionInCode position) : base(tokenType, lexeme, position)
        {
            this.z = z;
        }

        public ParseLeaf ToParseTree()
        {
            return LoadTokenFuncs[TokenType](this);
        }

        public Dictionary<TokenType, Func<Token, ParseLeaf>> LoadTokenFuncs => new()
        {
            [TokenType.Eof] = token => new ParseLeaf(token, z.Eof, ZI.Code.None),
            [TokenType.Int] = token => new ParseLeaf(token, z.Int,
                ZI.Code.Operand(new ZI.Constant<int>(int.Parse(Lexeme)))),
            [TokenType.Float] = token => new ParseLeaf(token, z.Float,
                ZI.Code.Operand(new ZI.Constant<double>(double.Parse(Lexeme)))),
            [TokenType.Char] = token => new ParseLeaf(token, z.Char,
                ZI.Code.Operand(new ZI.Constant<char>(Lexer<Token, TokenType>.DoEscapes(Lexeme)[0]))),
            [TokenType.Str] = token => new ParseLeaf(token, z.String,
                ZI.Code.Operand(new ZI.Constant<string>(Lexer<Token, TokenType>.DoEscapes(Lexeme)))),
            [TokenType.Id] = token =>
            {
                SymbolTableEntry entry = z.symbolTable.Get(token.Lexeme);
                (IType type, ZI.IOperand storage) = entry is null ? (z.UnknownIdentifier(token.Lexeme), null) : (entry.type, entry.storage);
                return new ParseLeaf(token, type, ZI.Code.Operand(storage));
            }
        };

        public override string ToString()
        {
            return TokenType == TokenType.Eof ? "<eof>" : Lexeme.Contains('\n') ? "<multi-line token>" : $"'{Lexeme}'";
        }

        public static TokenCreator<Token, TokenType> DefaultTokenCreator(DefaultExtension z)
        {
            return new TokenCreator<Token, TokenType>
            (
                eof: (position) => new Token(z, TokenType.Eof, "", position),
                //whitespace: (lexeme, position) => new Token(z, TokenType.Ws, lexeme, position),
                @new: (tokenType, lexeme, position) => new Token(z, tokenType, lexeme, position),
                eofType: TokenType.Eof,
                ignoreGroupName: "ignore"
            );
        }
    }
}
