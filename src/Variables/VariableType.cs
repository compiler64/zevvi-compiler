using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZevviCompiler.Variables
{
    public class VariableType : PropertyType, IIdentifierType
    {
        public string Identifier { get; }

        public readonly IType innerType;
        public readonly ZI.Variable storage;

        public VariableType(VariablesExtension v, string identifier, IType innerType, ZI.Variable storage)
            : base(v.Z, $"Variable<{innerType}>", true, v.AssignmentMerge)
        {
            this.innerType = innerType;
            this.storage = storage;
            Identifier = identifier;
            Converter = v.VariableConverter;
        }
    }
}
