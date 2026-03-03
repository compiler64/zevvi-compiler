using System;
using System.Collections.Generic;
using System.Text;

namespace ZevviCompiler
{
    public interface IParseLeaf
    {
        public Token Token { get; set; }
    }
}
