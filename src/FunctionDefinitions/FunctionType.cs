using System;
using System.Collections.Generic;
using System.Text;
using ZevviCompiler.FunctionCalls;
using ZevviCompiler.OperatorSyntaxes;
using ZevviCompiler.Transitions;

namespace ZevviCompiler.FunctionDefinitions
{
    public class FunctionType : OperatorType
    {
        public readonly GetReturnTypeFunc GetReturnType;
        public readonly ZI.Triple codeToCall;

        public FunctionType(FunctionCallsExtension f, string name, ZI.Triple codeToCall, GetReturnTypeFunc getReturnType)
            : base(f.Z, name)
        {
            Transition = new NormalTransition(name, f.FunctionDict);
            MergeSubtrees = subtrees => f.FunctionCallMerge(subtrees, codeToCall, getReturnType);
            this.codeToCall = codeToCall;
            GetReturnType = getReturnType;
        }
    }
}
