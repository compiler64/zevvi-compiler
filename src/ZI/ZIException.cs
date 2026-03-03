using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZevviCompiler.ZI
{
    public class ZIException : ZevviException
    {
        public ZIException(string message) : base(message)
        {
        }
    }
}
