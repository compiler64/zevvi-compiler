using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler
{
    public class ZevviUnknownMemberError : ZevviParserError
    {
        public readonly string unknownIdentifier;

        public ZevviUnknownMemberError(string unknownMember, IList<Token> tokens)
            : base($"Unknown member: {unknownMember}.", tokens) { }
        
        public ZevviUnknownMemberError(string module, string unknownMember, IList<Token> tokens)
            : base($"Unknown member: {module}.{unknownMember}.", tokens) { }

    }
}
