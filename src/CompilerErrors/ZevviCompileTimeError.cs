using System;
using System.Collections.Generic;
using System.Text;
using MyLibraries.LexicalAnalyzer;

namespace ZevviCompiler
{
    public class ZevviCompileTimeError : ZevviException
    {
        public readonly PositionInCode startLocation;
        public readonly PositionInCode endLocation;

        public ZevviCompileTimeError(string message, PositionInCode startLocation, PositionInCode endLocation) : base(message)
        {
            this.startLocation = startLocation;
            this.endLocation = endLocation;
        }
    }
}
