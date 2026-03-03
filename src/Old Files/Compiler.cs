using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompilerOld
{
    public class Compiler : Manager
    {
        public Compiler()
        {
            DefaultInitializers.defaultInitializer(this);
            DefaultInitializers.compilerInitializer(this);
        }
    }
}
