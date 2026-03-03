using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler
{
    public class ZevviUnknownIdentifierError : ZevviParserError
    {
        public readonly string unknownIdentifier;

        public ZevviUnknownIdentifierError(string unknownIdentifier, IList<Token> tokens)
            : base($"Unknown identifier at {tokens[0].Position}: {unknownIdentifier}.", tokens)
        {

        }
    }
}
