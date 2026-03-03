using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler
{
    class UnknownEnumValueInternalException : ZevviInternalCompilerError
    {
        public UnknownEnumValueInternalException(string enumName) : base($"Error in Zevvi compiler: Unknown enum value of type {enumName}.")
        {
        }
    }
}
