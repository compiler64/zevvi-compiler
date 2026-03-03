using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompilerOld
{
    abstract class FutureRef
    {
        public abstract int GetRef(InitTools t, int loc);

        public static implicit operator FutureRef(int i)
        {
            return new IntRef(i);
        }

        public static implicit operator FutureRef(string s)
        {
            return new FutureTypeRef(s);
        }

        private class IntRef : FutureRef
        {
            private readonly int i;

            public override int GetRef(InitTools t, int loc)
            {
                return i;
            }

            public IntRef(int i)
            {
                this.i = i;
            }
        }

        private class FutureTypeRef : FutureRef
        {
            private readonly string typeName;

            public override int GetRef(InitTools t, int loc)
            {
                return t.GetTypeLoc(loc, typeName);
            }

            public FutureTypeRef(string typeName)
            {
                this.typeName = typeName;
            }
        }
    }
}
