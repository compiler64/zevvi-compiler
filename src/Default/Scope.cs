using System.Collections.Generic;

namespace ZevviCompiler
{
    public class Scope
    {
        public readonly Dictionary<string, SymbolTableEntry> entries = new();
        public int firstVarLoc;
        public int nextVarLoc;

        public Scope(int firstVarLoc, int nextVarLoc)
        {
            this.firstVarLoc = firstVarLoc;
            this.nextVarLoc = nextVarLoc;
        }

        public Scope(int firstVarLoc) : this(firstVarLoc, firstVarLoc)
        {
        }
    }
}
