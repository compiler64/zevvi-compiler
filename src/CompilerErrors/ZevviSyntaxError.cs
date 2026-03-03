using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ZevviCompiler
{
    public class ZevviSyntaxError : ZevviParserError
    {
        public ZevviSyntaxError(string message, IList<Token> tokens)
            : base($"Syntax error at {tokens[0].Position}: " + message, tokens)
        {
        }
    }
}
