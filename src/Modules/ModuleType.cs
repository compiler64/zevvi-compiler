using System;
using System.Collections.Generic;
using ZevviCompiler.Transitions;

namespace ZevviCompiler.Modules
{
    public class ModuleType : ZType
    {
        public Scope members;

        public ModuleType(DefaultExtension z) : base(z, "Module")
        {
            Transition = new NormalTransition("Module", z.NormalDict);
            Converter = z.NormalConverter;
        }
    }
}
