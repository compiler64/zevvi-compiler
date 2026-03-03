using System;
using System.Collections.Generic;
using ZevviCompiler.Transitions;

namespace ZevviCompiler.StructureTypes
{
    public class StructureType : ZType
    {
        public Scope members;

        public StructureType(DefaultExtension z, string name) : base(z, name)
        {
            Transition = new NormalTransition(name, z.NormalDict);
            Converter = z.NormalConverter;
        }
    }
}
