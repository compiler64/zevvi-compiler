using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler
{
    public class ZevviException : Exception
    {
        public ZevviException(string message) : base(message)
        {
        }
    }
}
