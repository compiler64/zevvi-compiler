using System;
using System.Collections.Generic;
using System.Text;
using ZevviCompiler.Transitions;

namespace ZevviCompiler.Variables
{
    public class TypeType : ZType
    {
        public readonly VariablesExtension v;
        public readonly IType innerType;

        public TypeType(VariablesExtension v, IType innerType) : base(v.Z, $"type {innerType}")
        {
            Transition = new RightPrecTransition(VariablesExtension.P_TYPE_R, $"type {innerType}", v.TypeDict);
            Converter = Z.NormalConverter;
            this.v = v;
            this.innerType = innerType;
        }

        public override bool Equals(object obj)
        {
            return obj is TypeType type &&
                   innerType.Equals(type.innerType);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(innerType);
        }
    }
}
