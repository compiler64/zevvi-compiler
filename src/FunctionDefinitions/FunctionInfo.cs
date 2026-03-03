using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler.FunctionDefinitions
{
    public record FunctionInfo(string Name, IType[] ParamTypes, IType ReturnType, ZI.Triple CallMark)
    {
        public int NextLocalVarNum { get; set; } = 0;
    }
    //{
        /*public readonly string name;
        public readonly ZType[] paramTypes;
        public readonly ZType returnType;
        public readonly int callMark;
        public readonly int returnMark;

        public FunctionInfo
        {
            this.name = name;
            this.paramTypes = paramTypes;
            this.returnType = returnType;
            this.callMark = callMark;
            this.returnMark = returnMark;
        }*/
    //}
}
