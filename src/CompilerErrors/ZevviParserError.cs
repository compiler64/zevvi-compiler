using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ZevviCompiler
{
    public class ZevviParserError : ZevviCompileTimeError
    {
        public readonly ImmutableArray<Token> tokens;

        public ZevviParserError(string message, IList<Token> tokens)
            : base(message, tokens[0].Position, tokens[^1].Position)
        {
            this.tokens = tokens.ToImmutableArray();
        }
    }
}
