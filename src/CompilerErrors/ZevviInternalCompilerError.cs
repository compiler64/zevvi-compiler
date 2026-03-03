using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler
{
    public class ZevviInternalCompilerError : ZevviException
    {
        public ZevviInternalCompilerError(string message) : base(message)
        {
        }
    }
}
