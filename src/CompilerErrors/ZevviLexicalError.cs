using MyLibraries.LexicalAnalyzer;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler
{
    public class ZevviLexicalError : ZevviCompileTimeError
    {
        public ZevviLexicalError(PositionInCode position)
            : base($"Zevvi lexical error at {position}; couldn't match token.", position, position)
        {
        }
    }
}
