using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler.Modules
{
    public class ModuleScope : Scope
    {
        public ModuleScope(int firstVarLoc) : base(firstVarLoc) { }

        public ModuleScope(int firstVarLoc, int nextVarLoc) : base(firstVarLoc, nextVarLoc) { }
    }
}
