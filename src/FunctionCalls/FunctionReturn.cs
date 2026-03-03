using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler.FunctionCalls
{
    public readonly struct FunctionReturn
    {
        public readonly IType type;
        public readonly ZI.CodeFunc[] converts;

        public FunctionReturn(IType type, ZI.CodeFunc[] converts)
        {
            this.type = type;
            this.converts = converts;
        }

        public void Deconstruct(out IType returnType, out ZI.CodeFunc[] converts)
        {
            returnType = type;
            converts = this.converts;
        }
    }
}
