using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ZevviCompiler
{
    public class ZevviTypeError : ZevviParserError
    {
        public ZevviTypeError(string message, IList<Token> tokens)
            : base($"Type error at {tokens[0].Position}: " + message, tokens)
        {
        }
    }
}
