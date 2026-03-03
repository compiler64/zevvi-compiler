using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler.StructureTypes
{
    //[Obsolete("Structs will not have methods yet.")]
    public class StructScope : Scope
    {
        public StructScope(int firstVarLoc) : base(firstVarLoc) { }

        public StructScope(int firstVarLoc, int nextVarLoc) : base(firstVarLoc, nextVarLoc) { }
    }
}
